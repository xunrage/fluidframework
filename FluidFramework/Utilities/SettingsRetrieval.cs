using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// This class retrieves the XML settings of an application.
    /// </summary>
    public class SettingsRetrieval
    {
        #region FileNode Inner Class
        private class FileNode
        {
            private string filename;
            private Version version; // major.minor.build.revision

            public string FileName
            {
                get
                {
                    return filename;
                }
            }

            public Version FileVersion
            {
                get
                {
                    return version;
                }
            }

            public FileNode(string fileName)
            {
                filename = fileName;
                string[] parts = filename.Split('\\', '/');
                if (parts.Length > 1)
                {
                    try
                    {
                        version = new Version(parts[parts.Length - 2]);
                    }
                    catch
                    {
                        version = new Version("0.0.0.0");
                    }
                }
                else
                {
                    version = new Version("0.0.0.0");
                }
            }
        }
        #endregion

        #region SettingsRetrieval class
        private List<FileNode> retrieveList;
        private ApplicationSettingsBase settings;

        /// <summary>
        /// Constructor. Call with Properties.Settings.Default of the application.
        /// </summary>
        public SettingsRetrieval(ApplicationSettingsBase settings)
        {
            this.settings = settings;
        }

        private string ExecutablePath()
        {
            string executablePath = null;
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                Uri uri = new Uri(entryAssembly.CodeBase);
                executablePath = uri.IsFile ? string.Concat(uri.LocalPath, Uri.UnescapeDataString(uri.Fragment)) : uri.ToString();
            }
            return executablePath;            
        }

        private void ProcessDirectory(string sourceDir, int recursionLvl, int depthLvl)
        {
            if ((recursionLvl <= depthLvl) && (Directory.Exists(sourceDir)))
            {
                string executableFile = Path.GetFileName(ExecutablePath());
                if (String.IsNullOrEmpty(executableFile)) return;
                executableFile = executableFile.ToLower();
                if (sourceDir.ToLower().Contains(executableFile))
                {
                    // Process the list of files found in the directory.
                    string[] fileEntries = Directory.GetFiles(sourceDir);
                    foreach (string fileName in fileEntries)
                    {
                        string shortFileName = Path.GetFileName(fileName);
                        if (shortFileName != null && shortFileName.ToLower() == "user.config")
                        {
                            FileNode node = new FileNode(fileName);
                            retrieveList.Add(node);
                        }                        
                    }
                }

                // Recurse into subdirectories of this directory.
                string[] subdirEntries = Directory.GetDirectories(sourceDir);
                foreach (string subdir in subdirEntries)
                {
                    // Do not iterate through reparse points
                    if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                    {
                        if (subdir.ToLower().Contains(executableFile))
                        {
                            ProcessDirectory(subdir, recursionLvl + 1, depthLvl);
                        }
                    }
                }
            }
        }

        private void CreateFileList()
        {
            retrieveList = new List<FileNode>();
            string executableFile = Path.GetFileNameWithoutExtension(ExecutablePath());
            if (String.IsNullOrEmpty(executableFile)) return;
            string startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), executableFile.ToLower());
            ProcessDirectory(startPath, 1, 20);
        }

        private void RemoveOldSettings()
        {
            if (retrieveList.Count > 0)
            {
                foreach (FileNode node in retrieveList)
                {
                    if(!node.FileVersion.Equals(new Version("0.0.0.0")))
                    {
                        string targetPath = Path.GetDirectoryName(Path.GetFullPath(node.FileName));
                        if (targetPath == null) continue;
                        if (Directory.Exists(targetPath))
                        {
                            try
                            {
                                string[] fileEntries = Directory.GetFiles(targetPath);
                                foreach (string fileName in fileEntries)
                                {
                                    File.Delete(fileName);
                                }
                                Directory.Delete(targetPath, true);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private bool ProcessSetting(string settingName, string settingValue)
        {
            bool settingExists = settings.Properties.Cast<SettingsProperty>().Any(s => s.Name == settingName);
            if (settingExists)
            {
                switch (settings[settingName].GetType().FullName)
                {
                    case "System.String":
                        try
                        {
                            settings[settingName] = Convert.ToString(settingValue);
                            return true;
                        }
                        catch{ return false; }
                    case "System.Boolean":
                        try
                        {
                            settings[settingName] = Convert.ToBoolean(settingValue);
                            return true;
                        }
                        catch{ return false; }
                    case "System.Int32":
                        try
                        {
                            settings[settingName] = Convert.ToInt32(settingValue);
                            return true;
                        }
                        catch{ return false; }
                }                
            }
            return false;
        }

        private bool ReadSettings(FileNode target)
        {
            bool tagConfiguration = false;
            bool tagUserSettings = false;
            bool tagPropertiesSettings = false;
            bool tagSetting = false;
            string executableFile = Path.GetFileNameWithoutExtension(ExecutablePath());
            if (String.IsNullOrEmpty(executableFile)) return false;
            string tagPropertiesSettingsName = executableFile.ToLower() + ".properties.settings";
            string settingName = string.Empty;
            string settingValue = string.Empty;
            bool readNextText = false;
            int depthLevel = 0;
            try
            {
                XmlTextReader reader = new XmlTextReader(target.FileName);

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        // Element
                        case XmlNodeType.Element:
                            if ((depthLevel == 0) && (reader.Name.ToLower() == "configuration"))
                            {
                                tagConfiguration = true;
                                depthLevel++;
                                break;
                            }
                            if (tagConfiguration && (reader.Name.ToLower() == "usersettings"))
                            {
                                tagUserSettings = true;
                                depthLevel++;
                                break;
                            }
                            if (tagConfiguration && tagUserSettings && (reader.Name.ToLower() == tagPropertiesSettingsName))
                            {
                                tagPropertiesSettings = true;
                                depthLevel++;
                                break;
                            }
                            if (tagConfiguration && tagUserSettings && tagPropertiesSettings)
                            {
                                if (reader.Name.ToLower() == "setting")
                                {
                                    tagSetting = true;
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name.ToLower() == "name")
                                        {
                                            settingName = reader.Value;
                                        }
                                    }
                                    depthLevel++;
                                    break;
                                }
                                if (tagSetting && (reader.Name.ToLower() == "value"))
                                {
                                    readNextText = true;
                                    depthLevel++;
                                    break;
                                }
                            }
                            depthLevel++;
                            break;
                        // Text
                        case XmlNodeType.Text:
                            if (readNextText)
                            {
                                settingValue = reader.Value;
                                readNextText = false;
                            }
                            break;
                        // End Element
                        case XmlNodeType.EndElement:
                            if (tagConfiguration && tagUserSettings && tagPropertiesSettings)
                            {
                                if (tagSetting && (reader.Name.ToLower() == "value"))
                                {
                                    if (!ProcessSetting(settingName, settingValue))
                                    {
                                        throw new Exception("Invalid setting.");
                                    }
                                    settingName = string.Empty;
                                    settingValue = string.Empty;
                                    depthLevel--;
                                    break;
                                }
                                if (reader.Name.ToLower() == "setting")
                                {
                                    tagSetting = false;
                                    settingName = string.Empty;
                                    settingValue = string.Empty;
                                    depthLevel--;
                                    break;
                                }
                            }
                            if (tagConfiguration && tagUserSettings && (reader.Name.ToLower() == tagPropertiesSettingsName))
                            {
                                tagPropertiesSettings = false;
                                depthLevel--;
                                break;
                            }
                            if (tagConfiguration && (reader.Name.ToLower() == "usersettings"))
                            {
                                tagUserSettings = false;
                                depthLevel--;
                                break;
                            }
                            if ((depthLevel == 1) && (reader.Name.ToLower() == "configuration"))
                            {
                                tagConfiguration = false;
                                depthLevel--;
                                break;
                            }
                            depthLevel--;
                            break;
                    }
                }
                reader.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the settings for a previous version.
        /// </summary>
        public bool Retrieve(out string error)
        {
            CreateFileList();

            FileNode lastVersionNode = new FileNode("");
            foreach (FileNode node in retrieveList)
            {
                if (node.FileVersion.CompareTo(lastVersionNode.FileVersion) > 0)
                {
                    lastVersionNode = node;
                }
            }

            if (lastVersionNode.FileVersion.Equals(new Version("0.0.0.0")))
            {
                error = "A suitable previous version configuration file was not found in the system. Please configure the application.";
                return false;
            }

            if (!ReadSettings(lastVersionNode))
            {
                error = "A problem occurred while reading the configuration file. Please configure the application.";
                return false;
            }

            RemoveOldSettings();
            error = "";
            return true;
        }
        #endregion
    }
}

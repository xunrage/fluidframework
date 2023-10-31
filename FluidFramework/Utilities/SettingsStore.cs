using System;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Provides a mechanism to persist settings.
    /// </summary>
    public class SettingsStore : SecurityBase
    {
        #region Members
        private DataSet _dataset;
        #endregion

        #region Constructor
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsStore()
        {
            SettingsFile = @"settings.dat";
            SettingsPath = null;
            UseIsolatedStorage = false;
            IsLoaded = false;
            HasChanges = false;
        }

        #endregion

        #region Properties
        /// <summary>
        /// The file where the settings are stored.
        /// </summary>
        public string SettingsFile { get; set; }

        /// <summary>
        /// The path where the settings file is stored.
        /// </summary>
        public string SettingsPath { get; set; }

        /// <summary>
        /// Indicates that the settings file is saved in the isolated storage.
        /// </summary>
        public bool UseIsolatedStorage { get; set; }

        /// <summary>
        /// Signals if the internal dataset has been loaded.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Signals if the settings values have changed.
        /// </summary>
        public bool HasChanges { get; private set; }

        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the settings from the file in the internal dataset.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Load()
        {
            try
            {
                _dataset = null;
                IsLoaded = false;
                string content = null;

                if (UseIsolatedStorage)
                {
                    IsolatedStorageFile store = GetIsolatedStore();
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(SettingsFile, FileMode.Open, store))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            content = reader.ReadToEnd();
                            reader.Close();
                        }
                        stream.Close();
                    }
                }
                else
                {
                    string filePath = !String.IsNullOrEmpty(SettingsPath) ? Path.Combine(SettingsPath, SettingsFile) : SettingsFile;
                    if (File.Exists(filePath))
                    {
                        using (StreamReader reader = File.OpenText(filePath))
                        {
                            content = reader.ReadToEnd();
                            reader.Close();
                        }
                    }
                }

                IsLoaded = LoadContent(content);
                return IsLoaded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the settings from the content string in the internal dataset.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Load(string content)
        {
            try
            {
                _dataset = null;
                IsLoaded = false;
                IsLoaded = LoadContent(content);
                return IsLoaded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the settings from the internal dataset to the file.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Save()
        {
            try
            {
                if (_dataset == null) return false;
                string content = SaveContent();

                if (UseIsolatedStorage)
                {
                    IsolatedStorageFile store = GetIsolatedStore();
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(SettingsFile, FileMode.Create, store))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(content);
                            writer.Close();
                        }
                        stream.Close();
                    }
                }
                else
                {
                    string filePath = !String.IsNullOrEmpty(SettingsPath) ? Path.Combine(SettingsPath, SettingsFile) : SettingsFile;
                    using (FileStream stream = new FileStream(filePath, File.Exists(filePath) ? FileMode.Truncate : FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(content);                            
                            writer.Close();
                        }
                        stream.Close();
                    }
                }
                
                HasChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the settings from the internal dataset to the content string.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Save(out string content)
        {
            content = "";
            try
            {                
                if (_dataset == null) return false;
                content = SaveContent();
                HasChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a setting value by its name.
        /// Returns null if the value cannot be fetched.
        /// </summary>
        public dynamic Get(string name)
        {
            if (_dataset == null) return null;
            try
            {
                DataRow row = GetFirstRow(_dataset.Tables["Settings"], "Name='" + name + "'");
                if (row == null) return null;
                switch (row["Type"].ToString())
                {
                    case "Boolean":
                        return Convert.ToBoolean(row["Value"]);
                    case "Byte":
                        return Convert.ToByte(row["Value"]);
                    case "SByte":
                        return Convert.ToSByte(row["Value"]);
                    case "Char":
                        return Convert.ToChar(row["Value"]);
                    case "Decimal":
                        return Convert.ToDecimal(row["Value"]);
                    case "Double":
                        return Convert.ToDouble(row["Value"]);
                    case "Single":
                        return Convert.ToSingle(row["Value"]);
                    case "Int32":
                        return Convert.ToInt32(row["Value"]);
                    case "UInt32":
                        return Convert.ToUInt32(row["Value"]);
                    case "Int64":
                        return Convert.ToInt64(row["Value"]);
                    case "UInt64":
                        return Convert.ToUInt64(row["Value"]);
                    case "Int16":
                        return Convert.ToInt16(row["Value"]);
                    case "UInt16":
                        return Convert.ToUInt16(row["Value"]);
                    case "String":
                        return row["Value"].ToString();
                }
            }
            catch(Exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Sets a setting value by its name.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Set(string name, object value)
        {
            try
            {
                if (_dataset == null)
                {
                    _dataset = CreateDataset();
                    HasChanges = true;
                }
                Type type = value.GetType();
                DataRow row = GetFirstRow(_dataset.Tables["Settings"], "Name='" + name + "'");
                if (row == null)
                {
                    row = _dataset.Tables["Settings"].NewRow();
                    row["Name"] = name;
                    row["Type"] = type.Name;
                    row["Value"] = value.ToString();
                    _dataset.Tables["Settings"].Rows.Add(row);
                    HasChanges = true;
                }
                else
                {
                    if (row["Type"].ToString() != type.Name)
                    {
                        row.Delete();
                        row = _dataset.Tables["Settings"].NewRow();
                        row["Name"] = name;
                        row["Type"] = type.Name;
                        row["Value"] = value.ToString();
                        _dataset.Tables["Settings"].Rows.Add(row);
                        HasChanges = true;
                    }
                    else
                    {
                        if (row["Value"].ToString() != value.ToString())
                        {
                            row["Value"] = value.ToString();
                            HasChanges = true;
                        }
                    }
                }
                _dataset.AcceptChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a setting by its name.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Remove(string name)
        {
            try
            {
                if (_dataset == null) return false;
                DataRow row = GetFirstRow(_dataset.Tables["Settings"], "Name='" + name + "'");
                if (row == null) return false;
                row.Delete();
                HasChanges = true;
                _dataset.AcceptChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Private Methods
        private DataRow GetFirstRow(DataTable table, string where = "")
        {
            DataRow[] rows = table.Select(where, "", DataViewRowState.CurrentRows);
            return rows.Length > 0 ? rows[0] : null;
        }

        private int HexValue(byte b)
        {
            return b >= 48 && b <= 57 ? b - 48 : b - 55;
        }

        private string Rotate(byte[] key)
        {
            for (int i = 1; i < key.Length; i++)
            {
                int v = HexValue(key[i]) + HexValue(key[i-1]);
                if (v > 15) v -= 16;
                key[i] = v >= 0 && v <= 9 ? (byte)(v + 48) : (byte)(v + 55);
            }
            return Encoding.UTF8.GetString(key);
        }

        

        private DataSet CreateDataset()
        {
            DataTable table = new DataTable("Settings");
            DataColumn column = new DataColumn("Name", typeof(string), null, MappingType.Element);
            table.Columns.Add(column);
            table.Constraints.Add(new UniqueConstraint("PrimaryKey", new DataColumn[] { column }, true));
            column.AllowDBNull = false;
            column.Unique = true;
            column = new DataColumn("Type", typeof(string), null, MappingType.Element);
            table.Columns.Add(column);
            column = new DataColumn("Value", typeof(string), null, MappingType.Element);
            table.Columns.Add(column);
            DataSet dataset = new DataSet("SettingsDataset");
            dataset.Tables.Add(table);
            return dataset;
        }

        private bool LoadContent(string content)
        {
            if (!String.IsNullOrEmpty(content))
            {
                byte[] contentBytes = Convert.FromBase64String(content);
                byte[] key = new byte[32];
                for (var i = 0; i < 32; i++)
                {
                    key[i] = contentBytes[i];
                }
                byte[] data = new byte[contentBytes.Length - 32];
                for (var i = 32; i < contentBytes.Length; i++)
                {
                    data[i - 32] = contentBytes[i];
                }
                content = Encoding.UTF8.GetString(DecryptData(data, Rotate(key)));

                _dataset = CreateDataset();
                _dataset.ReadXml(new XmlTextReader(new StringReader(content)), XmlReadMode.InferSchema);
                return true;
            }
            return false;
        }

        private string SaveContent()
        {
            string content = _dataset.GetXml();
            string key = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            string password = Rotate(Encoding.UTF8.GetBytes(key));

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = EncryptData(Encoding.UTF8.GetBytes(content), password);

            byte[] fullBytes = new byte[keyBytes.Length + dataBytes.Length];
            Array.Copy(keyBytes, 0, fullBytes, 0, keyBytes.Length);
            Array.Copy(dataBytes, 0, fullBytes, keyBytes.Length, dataBytes.Length);
            return Convert.ToBase64String(fullBytes);
        }

        private IsolatedStorageFile GetIsolatedStore()
        {
            IsolatedStorageFile store;
            try
            {
                // ClickOnce application
                store = IsolatedStorageFile.GetUserStoreForApplication();
            }
            catch
            {
                store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null);
            }
            return store;
        }

        #endregion
    }
}
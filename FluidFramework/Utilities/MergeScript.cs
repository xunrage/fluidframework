using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// This class performs the merging of all the SQL files found in a directory.
    /// </summary>
    public static class MergeScript
    {
        private class Item
        {
            public string Name { get; set; }
            public int Priority { get; set; }
            public string FilePath { get; set; }
            public string Cache { get; set; }
            public List<string> Usages { get; set; }

            public Item()
            {
                Name = String.Empty;
                Priority = 0;
                FilePath = String.Empty;
                Cache = null;
                Usages = new List<string>();
            }
        }

        private static List<string> PrioritizeFileList(List<string> files)
        {
            List<string> result = new List<string>();
            try
            {
                List<string> masks = new List<string>
                {
                    @"\s*create\s+procedure\s+",
                    @"\s*create\s+function\s+",
                    @"\s*create\s+trigger\s+",
                    @"\s*create\s+proc\s+"
                };

                RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

                // Builds the item list
                List<Item> items = new List<Item>();
                foreach (string file in files)
                {
                    Item item = new Item()
                    {
                        FilePath = file
                    };

                    string fileContent = File.ReadAllText(file);

                    MatchCollection matches = null;
                    foreach (string mask in masks)
                    {
                        matches = Regex.Matches(fileContent, mask, options);
                        if (matches.Count == 1) break;
                    }

                    if (matches != null && matches.Count == 1)
                    {
                        string portion = fileContent.Substring(matches[0].Index + matches[0].Length);
                        Regex r = new Regex("\\s*[a-zA-Z0-9@\\$#_\\[\\]\\\"\\.]*", options);
                        Match m = r.Match(portion);
                        if (m.Success)
                        {
                            string name = portion.Substring(m.Index, m.Length).Trim().Replace("[", "").Replace("]", "").Replace("\"", "");
                            int x = name.IndexOf(".", StringComparison.Ordinal);
                            if (x >= 0)
                            {
                                name = name.Substring(x + 1);
                            }
                            name = name.Trim().ToLower();
                            item.Name = name;
                            item.Cache = portion.Substring(m.Index + m.Length);
                        }
                    }
                    items.Add(item);
                }

                // Builds active item list
                List<Item> activeItems = items.Where(item => !String.IsNullOrEmpty(item.Name)).ToList();

                // Looks for usages
                foreach (Item item in activeItems)
                {
                    foreach (Item uitem in activeItems)
                    {
                        if (uitem.Name != item.Name)
                        {
                            Regex r = new Regex(Regex.Escape(uitem.Name) + "[^a-zA-Z0-9@\\$#_]", options);
                            Match m = r.Match(item.Cache);
                            if (m.Success)
                            {
                                if (!item.Usages.Contains(uitem.Name)) item.Usages.Add(uitem.Name);
                            }
                        }
                    }
                    item.Cache = null;
                }

                // Computes the priority
                int level = 0;
                List<Item> levelItems;
                do
                {
                    level++;
                    levelItems = new List<Item>();
                    foreach (Item item in activeItems)
                    {
                        if (item.Priority > 0) continue;
                        int k = 0;
                        foreach (Item uitem in activeItems)
                        {
                            if (uitem.Priority > 0) continue;
                            if (uitem.Usages.Any(n => n == item.Name))
                            {
                                k++;
                            }
                            if (k > 0) break;
                        }
                        if (k == 0)
                        {
                            levelItems.Add(item);
                        }
                    }
                    if (levelItems.Count > 0)
                    {
                        foreach (Item item in levelItems)
                        {
                            item.Priority = level;
                        }
                    }
                } while (levelItems.Count > 0);

                foreach (Item item in activeItems)
                {
                    if (item.Priority == 0) item.Priority = level;
                }

                // Builds the result
                do
                {
                    levelItems = items.Where(item => item.Priority == level).ToList();
                    if (levelItems.Count > 0)
                    {
                        result.AddRange(levelItems.OrderBy(item=>item.Name).Select(item=>item.FilePath));
                    }
                    level--;
                } while (level >= 0);
            }
            catch
            {
                result.Clear();
            }
            return result;
        }

        /// <summary>
        /// Gets the default content separator.
        /// </summary>
        public static string SqlBatchSeparator
        {
            get
            {
                return Environment.NewLine + "GO" + Environment.NewLine;
            }
        }
        
        /// <summary>
        /// Merges together all the files found recursively in the source path.
        /// </summary>
        public static string MergeFiles(string sourcePath, string extension, List<string> contained, List<string> excluded, string separator, bool prioritize)
        {
            StringBuilder result = new StringBuilder();
            try
            {
                // Collects the files batch to merge
                List<string> filesBatch = new List<string>();
                string[] files = Directory.GetFiles(sourcePath, "*." + extension, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file) != "." + extension) continue;
                    bool addFile = false;
                    if (contained.Count > 0)
                    {                        
                        foreach (string filter in contained)
                        {
                            if (file.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(filter.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                            {
                                addFile = true;
                            }
                        }
                        if (!addFile) continue;
                    }
                    addFile = true;
                    if (excluded.Count > 0)
                    {
                        foreach (string filter in excluded)
                        {
                            if (file.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(filter.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                            {
                                addFile = false;
                            }
                        }
                        if (!addFile) continue;
                    }
                    filesBatch.Add(file);
                }

                // Change prioritization of the files
                if (prioritize)
                {
                    filesBatch = PrioritizeFileList(filesBatch);
                }

                foreach (string file in filesBatch)
                {
                    try
                    {
                        result.Append(File.ReadAllText(file));
                        result.Append(separator);
                    }
                    catch
                    {
                        result.Append("File merge error: " + file + separator);
                    }
                }
            }
            catch
            {
                result.Clear();
            }
            return result.ToString();
        }
    }
}
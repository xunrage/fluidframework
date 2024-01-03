using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Provides a mechanism to persist a dictionary.
    /// </summary>
    public class FastStore
    {
        /// <summary>
        /// The file where the dictionary is stored.
        /// </summary>
        public string StoreFile { get; set; }

        /// <summary>
        /// The path where the dictionary file is stored.
        /// </summary>
        public string StorePath { get; set; }

        /// <summary>
        /// Signals if the internal dictionary has been loaded.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Signals if the dictionary values have changed.
        /// </summary>
        public bool HasChanges { get; private set; }

        /// <summary>
        /// File signature
        /// </summary>
        public const string Signature = "XRDS";

        /// <summary>
        /// Internal class used for an element of the dictionary.
        /// </summary>
        private class ElementStore
        {
            public string CurrentValue;
            public string OriginalValue;
            public int Index;
        }

        /// <summary>
        /// Internal dictionary
        /// </summary>
        private Dictionary<string, ElementStore> elements;

        /// <summary>
        /// Bytes used to mask the dictionary content in the file.
        /// </summary>
        private byte[] byteMask;

        /// <summary>
        /// Keeps track of whether the file should be rewritten.
        /// </summary>
        private bool requireRepack;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FastStore()
        {
            StoreFile = @"settings.xrd";
            StorePath = null;

            Reset();            
        }

        /// <summary>
        /// Resets the state of the instance.
        /// </summary>
        private void Reset()
        {
            IsLoaded = false;
            HasChanges = false;

            elements = new Dictionary<string, ElementStore>();

            byteMask = new byte[8];
            for (int i = 0; i < byteMask.Length; i++) { byteMask[i] = 0; }

            requireRepack = true;
        }

        /// <summary>
        /// Gets a value by its name.
        /// Returns null if the value cannot be fetched.
        /// </summary>
        public string Get(string name)
        {
            try
            {
                if (String.IsNullOrEmpty(name)) return null;
                if (elements.ContainsKey(name))
                {
                    return elements[name].CurrentValue;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }            
        }

        /// <summary>
        /// Sets a value by its name.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Set(string name, string value)
        {
            try
            {
                if (String.IsNullOrEmpty(name)) return false;
                if (value == null) value = String.Empty;

                if (elements.ContainsKey(name))
                {                    
                    elements[name].CurrentValue = value;
                    if (elements[name].OriginalValue == null ||
                        Encoding.UTF8.GetBytes(elements[name].CurrentValue).Length != 
                        Encoding.UTF8.GetBytes(elements[name].OriginalValue).Length)
                    {
                        requireRepack = true;
                    }
                }
                else
                {
                    ElementStore element = new ElementStore()
                    {
                        CurrentValue = value,
                        Index = elements.Count > 0 ? elements.Max(e => e.Value.Index) + 1 : 1,
                        OriginalValue = null
                    };

                    elements.Add(name, element);
                    requireRepack = true;
                }

                HasChanges = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a value by its name.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Remove(string name)
        {
            try
            {
                if (String.IsNullOrEmpty(name)) return false;
                if(!elements.ContainsKey(name)) return false;
                
                ElementStore oldElement = elements[name];
                elements.Remove(name);

                if (oldElement.OriginalValue != null)
                {
                    requireRepack = true;
                }
                else
                {
                    requireRepack = !elements.All(e => e.Value.OriginalValue != null && e.Value.OriginalValue == e.Value.CurrentValue);
                }
                               
                HasChanges = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Computes the full file name of the store and checks if the file exists.
        /// </summary>
        private string GetStoreFile(out bool exists)
        {
            string filePath = !String.IsNullOrEmpty(StorePath) ? Path.Combine(StorePath, StoreFile) : StoreFile;
            exists = filePath != null && File.Exists(filePath);
            return filePath;
        }

        /// <summary>
        /// Tests if the file has the correct signature.
        /// </summary>
        public bool HasSignature()
        {
            bool exists;
            string filePath = GetStoreFile(out exists);
            if(!exists) { return false; }

            byte[] buffer = new byte[4];

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(buffer, 0, 4) != 4) return false;
            }

            byte[] signature = Encoding.UTF8.GetBytes(Signature);

            for(int i=0; i<4; i++)
            {
                if (signature[i] != buffer[i]) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Masks the bytes.
        /// </summary>
        private void MaskBytes(byte[] contentBytes)
        {
            int k = 0;
            for (int i = 0; i < contentBytes.Length; i++)
            {
                contentBytes[i] = (byte)(contentBytes[i] ^ byteMask[k]);
                k++;
                if (k > 7) k = 0;
            }
        }

        /// <summary>
        /// Loads a string from the file by reading the bytes that correspond to the size and content.
        /// </summary>
        private string LoadSegment(FileStream fs)
        {
            int length = 0;
            byte[] size = new byte[4];
            if (fs.Read(size, 0, 4) == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    length = length * 256 + size[i];
                }
            }
            else
            {
                return null;
            }

            if(length == 0)
            {
                return String.Empty;
            }

            byte[] content = new byte[length];
            if (fs.Read(content, 0, length) == length)
            {
                MaskBytes(content);
            }
            else
            {
                return null;
            }

            return Encoding.UTF8.GetString(content);
        }

        /// <summary>
        /// Loads the internal dictionary from the file.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Load()
        {
            try
            {
                Reset();               

                bool exists;
                string filePath = GetStoreFile(out exists);
                if (!exists) { return false; }

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Signature
                    bool hasSignature = false;
                    byte[] buffer = new byte[4];
                    if(fs.Read(buffer, 0, 4) == 4)
                    {
                        byte[] signature = Encoding.UTF8.GetBytes(Signature);
                        bool isMatch = true;
                        for (int i = 0; i < 4; i++)
                        {
                            if (signature[i] != buffer[i]) { isMatch = false; }
                        }
                        if (isMatch)
                        {
                            hasSignature = true;
                        }
                    }

                    if (hasSignature)
                    {
                        // Byte mask
                        bool hasByteMask = false;
                        if(fs.Read(byteMask, 0, 8) == 8)
                        {
                            hasByteMask = true;
                        }

                        // Data
                        if (hasByteMask)
                        {
                            string name;
                            int index = 1;
                            do
                            {
                                name = LoadSegment(fs);

                                if (name != null)
                                {
                                    ElementStore element = new ElementStore()
                                    {
                                        CurrentValue = LoadSegment(fs),
                                        Index = index
                                    };
                                    if (element.CurrentValue != null)
                                    {
                                        element.OriginalValue = element.CurrentValue;
                                        elements.Add(name, element);
                                        index++;
                                    }
                                }
                            }
                            while (name != null);

                            requireRepack = false;
                            IsLoaded = true;
                        }
                    }
                }
               
                return IsLoaded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves a string to the file by writing the bytes that correspond to the size and content.
        /// </summary>
        private void SaveSegment(FileStream fs, string value)
        {
            byte[] size = new byte[4];
            byte[] content = Encoding.UTF8.GetBytes(value);
            int length = content.Length;
            for (int i = 3; i >= 0; i--)
            {
                size[i] = (byte)(length % 256);
                length = length / 256;
            }

            MaskBytes(content);

            fs.Write(size, 0, 4);
            fs.Write(content, 0, content.Length);
        }

        /// <summary>
        /// Saves the internal dictionary to the file.
        /// Returns true if the operation was successful.
        /// </summary>
        public bool Save(bool forceRepack = false)
        {
            try
            {
                bool exists;
                string filePath = GetStoreFile(out exists);

                if (!exists)
                {
                    requireRepack = true;
                }
                
                if (requireRepack || forceRepack)
                {
                    new Random().NextBytes(byteMask);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] signature = Encoding.UTF8.GetBytes(Signature);
                        fs.Write(signature, 0, 4);
                        fs.Write(byteMask, 0, 8);

                        foreach(KeyValuePair<string, ElementStore> element in elements.OrderBy(e => e.Value.Index))
                        {
                            SaveSegment(fs, element.Key);
                            SaveSegment(fs, element.Value.CurrentValue);

                            element.Value.OriginalValue = element.Value.CurrentValue;
                        }
                    }
                }
                else
                {
                    int cutOffIndex = elements.Where(e => e.Value.CurrentValue != e.Value.OriginalValue)
                        .Select(e => e.Value.Index).DefaultIfEmpty(0).Max();

                    if (cutOffIndex > 0)
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                        {
                            fs.Seek(12, SeekOrigin.Begin);

                            foreach (KeyValuePair<string, ElementStore> element in elements.OrderBy(e => e.Value.Index))
                            {
                                if (element.Value.CurrentValue == element.Value.OriginalValue)
                                {
                                    fs.Seek(8 +
                                        Encoding.UTF8.GetBytes(element.Key).Length +
                                        Encoding.UTF8.GetBytes(element.Value.CurrentValue).Length, SeekOrigin.Current);
                                }
                                else
                                {
                                    SaveSegment(fs, element.Key);
                                    SaveSegment(fs, element.Value.CurrentValue);
                                }

                                element.Value.OriginalValue = element.Value.CurrentValue;

                                if (element.Value.Index == cutOffIndex) break;
                            }
                        }
                    }
                }               

                requireRepack = false;
                HasChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

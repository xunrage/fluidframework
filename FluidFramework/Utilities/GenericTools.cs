using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Tools to work with datasets.
    /// </summary>
    public static class GenericTools
    {        
        /// <summary>
        /// Cuts the string at the given length.
        /// </summary>
        public static string CutString(string s, int maxLength)
        {
            if (String.IsNullOrEmpty(s) || maxLength < 1) return String.Empty;
            s = s.Trim();
            if (s.Length > maxLength) return s.Substring(0, maxLength);
            return s;
        }

        /// <summary>
        /// Creates a string with the elements of the list separated.
        /// </summary>
        public static string Enumerate<T>(List<T> items, string separator = ",", string encloseStart = "", string encloseEnd = "")
        {
            StringBuilder result = new StringBuilder();
            if (items != null)
            {
                bool first = true;
                foreach (T item in items)
                {
                    if (first) first = false; else result.Append(separator);
                    result.Append(encloseStart);
                    result.Append(item);
                    result.Append(encloseEnd);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Converts a value to the given type.
        /// </summary>
        public static dynamic ConvertTo(object value, Type type)
        {
            switch (type.Name)
            {
                case "String":
                    return value.ToString();
                case "Int32":
                    return Convert.ToInt32(value);
                case "Boolean":
                    return Convert.ToBoolean(value);
                case "Decimal":
                    return Convert.ToDecimal(value);
                case "DateTime":
                    return Convert.ToDateTime(value);
                case "Byte":
                    return Convert.ToByte(value);
                case "SByte":
                    return Convert.ToSByte(value);
                case "Char":
                    return Convert.ToChar(value);
                case "Double":
                    return Convert.ToDouble(value);
                case "Single":
                    return Convert.ToSingle(value);
                case "UInt32":
                    return Convert.ToUInt32(value);
                case "Int64":
                    return Convert.ToInt64(value);
                case "UInt64":
                    return Convert.ToUInt64(value);
                case "Int16":
                    return Convert.ToInt16(value);
                case "UInt16":
                    return Convert.ToUInt16(value);
            }
            return value;
        }

        /// <summary>
        /// Splits a string value that has the elements separated into a list.
        /// </summary>
        public static List<T> SplitEnumeration<T>(string value, string separator = ",", bool keepEmpty = false,  bool distinct = false, SortOrder sort = SortOrder.None)
        {
            if (value == null) return null;
            string[] items = value.Split(new[] { separator }, keepEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<T> list = items.Select(i => (T)ConvertTo(i, typeof(T)));
            if (distinct)
            {
                list = list.Distinct();
            }
            switch (sort)
            {
                case SortOrder.Ascending:
                    list = list.OrderBy(i => i);
                    break;
                case SortOrder.Descending:
                    list = list.OrderByDescending(i => i);
                    break;
            }
            return list.ToList();
        }
    }
}

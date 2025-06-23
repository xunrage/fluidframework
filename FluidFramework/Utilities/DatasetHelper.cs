using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Offers helper methods to work with a dataset.
    /// </summary>
    public static class DatasetHelper
    {
        /// <summary>
        /// Clears the table content if it exists.
        /// </summary>
        public static void ClearTable(DataSet dataset, string tableName)
        {
            if (dataset.Tables.Contains(tableName))
            {
                dataset.Tables[tableName].Clear();
            }
        }

        /// <summary>
        /// Removes the table from the dataset if it exists.
        /// </summary>
        public static void RemoveTable(DataSet dataset, string tableName)
        {
            if (dataset.Tables.Contains(tableName))
            {
                dataset.Tables.Remove(tableName);
            }
        }

        /// <summary>
        /// Adds a data table to a dataset, removing the old existing one first.
        /// </summary>
        public static void AddTableToDataset(DataSet dataset, DataTable table)
        {
            RemoveTable(dataset, table.TableName);            
            dataset.Tables.Add(table);
        }

        /// <summary>
        /// Creates a data table that contains the given columns and values.
        /// </summary>
        public static DataTable CreateVirtualTable(string tableName, Dictionary<string, Type> columns, List<List<object>> values = null)
        {
            DataTable table = new DataTable(tableName);
            foreach (KeyValuePair<string,Type> column in columns)
            {
                table.Columns.Add(new DataColumn(column.Key, column.Value));
            }

            if (values != null)
            {
                foreach (List<object> rowValues in values)
                {
                    PushRow(table, rowValues);
                }
            }

            return table;
        }

        /// <summary>
        /// Adds a new row to the table with the given values.
        /// </summary>
        public static DataRow PushRow(DataTable table, List<object> values)
        {
            DataRow row = table.NewRow();
            PushValues(row, values);            
            table.Rows.Add(row);
            return row;
        }

        /// <summary>
        /// Fills the values for the given row.
        /// </summary>
        public static void PushValues(DataRow row, List<object> values)
        {
            int index = 0;
            foreach (object value in values)
            {
                if (value == null)
                {
                    row[index++] = DBNull.Value;
                }
                else
                {
                    row[index++] = value;
                }
            }
        }

        /// <summary>
        /// Converts the Guid, String and DateTime columns values from DBNull to Empty.
        /// </summary>
        public static void NullToEmpty(DataRow row, List<string> columns)
        {
            if (row == null) return;
            foreach (string column in columns)
            {
                if (row.IsNull(column))
                {
                    if (row.Table.Columns[column].DataType == typeof(Guid)) row[column] = Guid.Empty;
                    if (row.Table.Columns[column].DataType == typeof(String)) row[column] = String.Empty;
                    if (row.Table.Columns[column].DataType == typeof (DateTime)) row[column] = default(DateTime);
                }
            }
        }

        /// <summary>
        /// Converts the Guid, String and DateTime columns values from Empty to DBNull.
        /// </summary>
        public static void EmptyToNull(DataRow row, List<string> columns)
        {
            if (row == null) return;
            foreach (string column in columns)
            {
                if (!row.IsNull(column))
                {
                    if (row.Table.Columns[column].DataType == typeof(Guid) && (Guid)row[column] == Guid.Empty) row[column] = Convert.DBNull;
                    if (row.Table.Columns[column].DataType == typeof(String) && (String)row[column] == String.Empty) row[column] = Convert.DBNull;
                    if (row.Table.Columns[column].DataType == typeof (DateTime) && (DateTime) row[column] == default(DateTime)) row[column] = Convert.DBNull;
                }
            }
        }

        /// <summary>
        /// Changes the string field value if it is different from the previous value.
        /// </summary>
        public static void ModifyStringIfChanged(DataRow row, string field, string value)
        {
            if (row.IsNull(field))
            {
                if (!String.IsNullOrEmpty(value)) row[field] = value;
            }
            else
            {
                if (String.IsNullOrEmpty(value)) row[field] = DBNull.Value;
                else if (row[field].ToString() != value) row[field] = value;
            }
        }

        /// <summary>
        /// Checks if the field value differs from the given value.
        /// </summary>
        public static bool IsDifferent(DataRow row, string field, object value)
        {
            bool result = false;
            if (row.IsNull(field))
            {
                result = (value != null) && (value != DBNull.Value);
            }
            else
            {
                if (value == null || value == DBNull.Value)
                {
                    result = true;
                }
                else
                {
                    switch (value.GetType().Name)
                    {
                        case "String":
                            result = row[field].ToString() != value.ToString();
                            break;
                        case "Int32":
                            result = Convert.ToInt32(row[field]) != Convert.ToInt32(value);
                            break;
                        case "Boolean":
                            result = Convert.ToBoolean(row[field]) != Convert.ToBoolean(value);
                            break;
                        case "Decimal":
                            result = Convert.ToDecimal(row[field]) != Convert.ToDecimal(value);
                            break;
                        case "DateTime":
                            result = Convert.ToDateTime(row[field]) != Convert.ToDateTime(value);
                            break;
                        case "Byte":
                            result = Convert.ToByte(row[field]) != Convert.ToByte(value);
                            break;
                        case "SByte":
                            result = Convert.ToSByte(row[field]) != Convert.ToSByte(value);
                            break;
                        case "Char":
                            result = Convert.ToChar(row[field]) != Convert.ToChar(value);
                            break;
                        case "Double":
                            result = Math.Abs(Convert.ToDouble(row[field]) - Convert.ToDouble(value)) > 0.0000001;
                            break;
                        case "Single":
                            result = Math.Abs(Convert.ToSingle(row[field]) - Convert.ToSingle(value)) > 0.0000001;
                            break;
                        case "UInt32":
                            result = Convert.ToUInt32(row[field]) != Convert.ToUInt32(value);
                            break;
                        case "Int64":
                            result = Convert.ToInt64(row[field]) != Convert.ToInt64(value);
                            break;
                        case "UInt64":
                            result = Convert.ToUInt64(row[field]) != Convert.ToUInt64(value);
                            break;
                        case "Int16":
                            result = Convert.ToInt16(row[field]) != Convert.ToInt16(value);
                            break;
                        case "UInt16":
                            result = Convert.ToUInt16(row[field]) != Convert.ToUInt16(value);
                            break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Changes the field value if it is different from the previous value.
        /// </summary>
        public static void ModifyIfChanged(DataRow row, string field, object value)
        {
            if (IsDifferent(row, field, value))
            {
                if (value == null) row[field] = DBNull.Value; else row[field] = value;
            }
        }

        /// <summary>
        /// Changes the fields corresponding to the given parameters.
        /// </summary>
        public static void ChangeFieldsFromParameters(DataRow row, Dictionary<string, object> parameters)
        {
            foreach (KeyValuePair<string, object> p in parameters)
            {
                if (p.Value is String)
                {
                    ModifyStringIfChanged(row, p.Key, p.Value.ToString());
                }
                else
                {
                    ModifyIfChanged(row, p.Key, p.Value);
                }
            }
        }

        /// <summary>
        /// Sets the fields corresponding to the given parameters.
        /// </summary>
        public static void SetFieldsFromParameters(DataRow row, Dictionary<string, object> parameters)
        {
            foreach (KeyValuePair<string, object> p in parameters)
            {
                if (p.Value == null) row[p.Key] = DBNull.Value; else row[p.Key] = p.Value;
            }
        }

        /// <summary>
        /// Adds a column in Parameters table and fills the value on the first row.
        /// </summary>
        public static void AddParameter(DataSet dataset, string name, object value, Type hintType = null)
        {
            DataTable table;
            if (!dataset.Tables.Contains("Parameters"))
            {
                table = new DataTable("Parameters");
                dataset.Tables.Add(table);
            }
            else
            {
                table = dataset.Tables["Parameters"];
            }
            if (table.Columns.Contains(name)) table.Columns.Remove(name);
            if (hintType == null)
            {
                if (value == null)
                {
                    hintType = typeof(string);
                }
                else
                {
                    hintType = value.GetType();
                    if (!hintType.IsValueType && hintType != typeof(string))
                    {
                        hintType = hintType.GetEnumUnderlyingType();
                    }
                }
            }

            DataColumn column = new DataColumn(name, hintType);
            table.Columns.Add(column);
            DataRow row = table.AsEnumerable().FirstOrDefault();
            if (row == null)
            {
                row = table.NewRow();
                table.Rows.Add(row);
            }
            if (value == null) row[name] = DBNull.Value; else row[name] = value;
        }        

        /// <summary>
        /// Adds a column in Parameters table and fills the value on the first row, allowing the null value.
        /// </summary>
        public static void AddParameterWithNull(DataSet dataset, string name, object value, Type type)
        {
            if (value == null || String.IsNullOrEmpty(value.ToString()))
            {
                AddParameter(dataset, name, null, type);
            }
            else
            {
                try
                {
                    AddParameter(dataset, name, GenericTools.ConvertTo(value, type));
                }
                catch
                {
                    AddParameter(dataset, name, null, type);
                }
            }
        }

        /// <summary>
        /// Returns a list with all the column names of the given table.
        /// </summary>
        public static List<string> AllColumns(DataTable table)
        {
            if (table == null) return null;
            List<string> result = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                result.Add(column.ColumnName);
            }
            return result;
        }

        /// <summary>
        /// Returns a list with all the column names of the given table, excluding the undesired ones.
        /// </summary>
        public static List<string> AllColumnsWithout(DataTable table, List<string> excludedColumns)
        {
            if (excludedColumns == null) return AllColumns(table);
            List<string> result = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                if (!excludedColumns.Contains(column.ColumnName)) result.Add(column.ColumnName);
            }
            return result;
        }

        /// <summary>
        /// Creates a copy of the source row in the same data table with a different id.
        /// </summary>        
        public static DataRow CloneRow(DataRow srcRow, string idField, object idValue)
        {
            DataRow dstRow = srcRow.Table.NewRow();
            dstRow.ItemArray = (object[])srcRow.ItemArray.Clone();
            dstRow[idField] = idValue;
            srcRow.Table.Rows.Add(dstRow);
            return dstRow;
        }

        /// <summary>
        /// Finds the minimum available integer id.
        /// </summary>
        public static int FindVirtualIntId(DataTable table, string idField)
        {
            int minId = -1;

            if (table.Select().Length > 0)
            {
                minId = table.Select().Min(row => Convert.ToInt32(row[idField]));
                minId--;
                if (minId >= 0) minId = -1;
            }

            return minId;
        }

        /// <summary>
        /// Sets the value of the given field with the specified value or DBNull.Value.
        /// </summary>
        public static void SetNullOrValue(DataRow row, string field, object value)
        {
            if (row == null || !row.Table.Columns.Contains(field)) return;
            if (value == null)
            {
                row[field] = DBNull.Value;
            }
            else
            {
                row[field] = value;
            }
        }

        /// <summary>
        /// Gets the value of the given field if possible or null otherwise.
        /// </summary>
        public static object GetNullOrValue(DataRow row, string field)
        {
            if (row == null) return null;
            if (!row.Table.Columns.Contains(field)) return null;
            if (row.IsNull(field)) return null;
            return row[field];
        }

        /// <summary>
        /// Creates a new string data column in the given data table that contains a template of existing string columns.
        /// </summary>
        public static void ComposeColumn(DataSet dataset, string tableName, string columnName, Dictionary<string, string> mixedColumns, bool withAcceptChanges = true)
        {
            if (dataset.Tables[tableName].Columns.Contains(columnName))
            {
                dataset.Tables[tableName].Columns.Remove(columnName);
            }

            dataset.Tables[tableName].Columns.Add(new DataColumn(columnName, typeof(string)));

            foreach (DataRow row in dataset.Tables[tableName].Select())
            {
                StringBuilder value = new StringBuilder();

                foreach (KeyValuePair<string, string> item in mixedColumns)
                {
                    if (!row.IsNull(item.Key) && !String.IsNullOrEmpty(row[item.Key].ToString()))
                    {
                        value.Append(item.Value.Replace("%", row[item.Key].ToString()));
                    }
                }

                row[columnName] = value.ToString();
            }

            if (withAcceptChanges)
            {
                dataset.Tables[tableName].AcceptChanges();
            }
        }
    }
}

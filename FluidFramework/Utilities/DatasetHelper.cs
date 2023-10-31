using System;
using System.Collections.Generic;
using System.Data;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Offers helper methods to work with a dataset.
    /// </summary>
    public static class DatasetHelper
    {
        /// <summary>
        /// Adds a data table to a dataset, removing the old existing one first.
        /// </summary>
        public static void AddTableToDataset(DataSet dataset, DataTable table)
        {
            if (dataset.Tables.Contains(table.TableName))
            {
                dataset.Tables.Remove(table.TableName);
            }

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
        public static void PushRow(DataTable table, List<object> values)
        {
            DataRow row = table.NewRow();
            PushValues(row, values);            
            table.Rows.Add(row);
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
    }
}

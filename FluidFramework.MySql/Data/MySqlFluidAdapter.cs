using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using FluidFramework.Utilities;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate a MySqlDataAdapter.
    /// </summary>
    public class MySqlFluidAdapter
    {
        /// <summary>
        /// The dynamically generated MySqlDataAdapter.
        /// </summary>
        public MySqlDataAdapter Adapter { get; set; }

        /// <summary>
        /// Marks the start of the WHERE part of the SELECT statement.
        /// </summary>
        public bool ConditionStart { get; set; }

        /// <summary>
        /// Helper property to create a new FluidAdapter instance.
        /// </summary>
        public static MySqlFluidAdapter New
        {
            get
            {
                return new MySqlFluidAdapter();
            }
        }

        /// <summary>
        /// A class that encapsulates a SQL type.
        /// </summary>
        public class Hint
        {
            /// <summary>
            /// SqlDbType
            /// </summary>
            public MySqlDbType MySqlDbType { get; set; }

            /// <summary>
            /// Size in bytes
            /// </summary>
            public int Size { get; set; }

            /// <summary>
            /// Hint is used as default.
            /// </summary>
            public bool IsDefault { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public Hint() { }

            /// <summary>
            /// Constructor with parameters
            /// </summary>
            public Hint(MySqlDbType type, int size, bool isdefault = false)
            {
                MySqlDbType = type;
                Size = size;
                IsDefault = isdefault;
            }
        }

        /// <summary>
        /// The mapping between .NET types and MySQL types
        /// </summary>
        public Dictionary<Type, Hint> TypeHinting { get; set; }

        /// <summary>
        /// The mapping between fields and MySQL types
        /// </summary>
        public Dictionary<string, Hint> FieldHinting { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MySqlFluidAdapter()
        {
            Adapter = new MySqlDataAdapter();
            ConditionStart = true;
            CreateTypeHints();
            FieldHinting = new Dictionary<string, Hint>();
        }

        #region Private Methods

        private void CreateTypeHints()
        {
            TypeHinting = new Dictionary<Type, Hint>
            {
                {typeof(String), new Hint(MySqlDbType.VarChar, -1, true)},
                {typeof(Byte), new Hint(MySqlDbType.Byte, 1)},
                {typeof(Int16), new Hint(MySqlDbType.Int16, 2)},
                {typeof(Int32), new Hint(MySqlDbType.Int32, 4)},
                {typeof(Int64), new Hint(MySqlDbType.Int64, 8)},
                {typeof(DateTime), new Hint(MySqlDbType.DateTime, 8)},                
                {typeof(Decimal), new Hint(MySqlDbType.Decimal, 9)},
                {typeof(Boolean), new Hint(MySqlDbType.UInt16, 1)}
            };
        }

        private string GetConnector(string connector)
        {
            if (ConditionStart)
            {
                connector = "WHERE";
                ConditionStart = false;
            }
            return String.IsNullOrEmpty(connector) ? " " : " " + connector + " ";
        }

        private string SanitizeName(string name)
        {
            return name.Replace(" ", "_");
        }

        private void SetMapping(string datasetTableName, List<string> columns)
        {
            if (String.IsNullOrEmpty(datasetTableName)) return;
            if (columns == null)
            {
                Adapter.TableMappings.Add(new DataTableMapping("Table", datasetTableName));
                return;
            }
            
            DataColumnMapping[] mapping = new DataColumnMapping[columns.Count];
            int index = 0;
            foreach (string column in columns)
            {
                mapping[index] = new DataColumnMapping(column, column);
                index++;
            }
            Adapter.TableMappings.Add(new DataTableMapping("Table", datasetTableName, mapping));            
        }

        private void SetMapping(DataSet dataset, string datasetTableName)
        {
            if (String.IsNullOrEmpty(datasetTableName)) return;
            if (dataset == null || !dataset.Tables.Contains(datasetTableName))
            {
                Adapter.TableMappings.Add(new DataTableMapping("Table", datasetTableName));
                return;
            }

            DataColumnMapping[] mapping = new DataColumnMapping[dataset.Tables[datasetTableName].Columns.Count];
            int index = 0;
            foreach (DataColumn column in dataset.Tables[datasetTableName].Columns)
            {
                mapping[index] = new DataColumnMapping(column.ColumnName, column.ColumnName);
                index++;
            }
            Adapter.TableMappings.Add(new DataTableMapping("Table", datasetTableName, mapping));
        }

        private MySqlDbType GetDbType(DataColumn column)
        {
            if (FieldHinting.ContainsKey(column.ColumnName))
            {
                return FieldHinting[column.ColumnName].MySqlDbType;
            }
            if (TypeHinting.ContainsKey(column.DataType))
            {
                return TypeHinting[column.DataType].MySqlDbType;
            }
            return TypeHinting.FirstOrDefault(t => t.Value.IsDefault).Value.MySqlDbType;
        }

        private MySqlParameter CreateParameter(DataColumn column)
        {
            return new MySqlParameter("@" + SanitizeName(column.ColumnName), GetDbType(column), 0, column.ColumnName);
        }

        private MySqlParameter CreateInputOriginalParameter(DataColumn column)
        {
            return new MySqlParameter("@Original_" + SanitizeName(column.ColumnName), GetDbType(column), 0, ParameterDirection.Input, false, 0, 0, column.ColumnName, DataRowVersion.Original, null);
        }

        private string ExtraSelectText(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, bool isInsert)
        {
            try
            {
                if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
                DataColumn pkColumn = dataset.Tables[datasetTableName].PrimaryKey.FirstOrDefault();
                if (pkColumn == null) return "";
                string commandText = ";SELECT " + GenericTools.Enumerate(selectColumns, ", ", "`", "`") + " FROM `" + databaseTableName + "` WHERE ";
                if (isInsert && pkColumn.AutoIncrement)
                {
                    commandText += "(`" + pkColumn.ColumnName + "` = LAST_INSERT_ID())";
                }
                else
                {
                    commandText += "(`" + pkColumn.ColumnName + "` = @" + SanitizeName(pkColumn.ColumnName) + ")";
                }
                return commandText;
            }
            catch
            {
                return "";
            }
        }

        private string UpdateDeleteWhereText(DataSet dataset, string datasetTableName, List<string> whereColumns)
        {
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            StringBuilder result = new StringBuilder();
            bool first = true;
            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                if (first) first = false; else result.Append(" AND ");
                string line = "(`" + column.ColumnName + "` = @Original_" + SanitizeName(column.ColumnName) + ")";
                if (column.AllowDBNull)
                {
                    line = "((@Original_" + SanitizeName(column.ColumnName) + " IS NULL AND `" + column.ColumnName + "` IS NULL) OR " + line + ")";
                }
                result.Append(line);
            }
            return result.ToString();
        }

        private MySqlCommand CreateSelectCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            MySqlCommand command = new MySqlCommand
            {
                CommandText = "SELECT " + GenericTools.Enumerate(selectColumns, ", ", "`", "`") + " FROM `" + databaseTableName + "`",
                CommandType = CommandType.Text
            };
            return command;
        }

        private MySqlCommand CreateInsertCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            List<string> valueColumns = (from string column in selectColumns
                where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                select SanitizeName(column)).ToList();
            List<string> insertColumns = (from string column in selectColumns
                where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                select column).ToList();
            MySqlCommand command = new MySqlCommand
            {
                CommandText = "INSERT INTO `" + databaseTableName + "` (" + GenericTools.Enumerate(insertColumns, ", ", "`", "`") + ") " +
                              "VALUES (" + GenericTools.Enumerate(valueColumns, ", ", "@") + ")",
                CommandType = CommandType.Text
            };

            command.CommandText += ExtraSelectText(dataset, datasetTableName, databaseTableName, selectColumns, true);

            foreach (string columnName in insertColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateParameter(column));
            }
            return command;
        }

        private MySqlCommand CreateUpdateCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, List<string> whereColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            MySqlCommand command = new MySqlCommand
            {
                CommandText = "UPDATE `" + databaseTableName + "` SET " +
                              GenericTools.Enumerate((from string column in selectColumns
                                  where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                  select "`" + column + "` = @" + SanitizeName(column)).ToList(), ", ") +
                              " WHERE (" + UpdateDeleteWhereText(dataset, datasetTableName, whereColumns) + ")",
                CommandType = CommandType.Text
            };

            command.CommandText += ExtraSelectText(dataset, datasetTableName, databaseTableName, selectColumns, false);

            foreach (string columnName in (from string column in selectColumns where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false select column).ToList())
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateParameter(column));
            }

            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateInputOriginalParameter(column));
            }

            // Add primary key parameter used by extra select
            DataColumn pkColumn = dataset.Tables[datasetTableName].PrimaryKey.FirstOrDefault();
            if (pkColumn != null)
            {
                if (!command.Parameters.Contains("@" + SanitizeName(pkColumn.ColumnName)))
                {
                    command.Parameters.Add(CreateParameter(pkColumn));
                }
            }

            return command;
        }

        private MySqlCommand CreateDeleteCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> whereColumns)
        {
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            MySqlCommand command = new MySqlCommand
            {
                CommandText = "DELETE FROM `" + databaseTableName + "` WHERE (" + UpdateDeleteWhereText(dataset, datasetTableName, whereColumns) + ")",
                CommandType = CommandType.Text
            };

            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateInputOriginalParameter(column));
            }
            return command;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a select command and sets the table column mapping.
        /// </summary>
        public MySqlFluidAdapter CreateSelect(string databaseTableName, List<string> selectColumns, string tableAlias = null, string datasetTableName = null)
        {
            ConditionStart = true;
            if (datasetTableName == null) datasetTableName = databaseTableName;
            SetMapping(datasetTableName, selectColumns);
            Adapter.SelectCommand = new MySqlCommand("SELECT " + (selectColumns == null || selectColumns.Count < 1 ? (tableAlias != null ? tableAlias + "." : "") + "*" :
                                                     GenericTools.Enumerate(selectColumns, ", ", (tableAlias != null ? tableAlias + "." : "") + "`", "`")) +
                                                     " FROM `" + databaseTableName + "`" + (tableAlias != null ? " AS " + tableAlias : ""));
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        /// <summary>
        /// Creates a select command and sets the table column mapping.
        /// </summary>
        public MySqlFluidAdapter CreateSelect(DataTable table, List<string> selectColumns = null, string tableAlias = null, string databaseTableName = null)
        {
            if (selectColumns == null) selectColumns = GenericTools.AllColumns(table);
            if (databaseTableName == null) databaseTableName = table.TableName;
            return CreateSelect(databaseTableName, selectColumns, tableAlias, table.TableName);
        }

        /// <summary>
        /// Creates a select command with the given verbatim command text and sets the table column mapping.
        /// </summary>
        public MySqlFluidAdapter CreateSelect(string commandText, string datasetTableName, List<string> selectColumns = null, CommandType commandType = CommandType.Text)
        {
            ConditionStart = true;
            SetMapping(datasetTableName, selectColumns);
            Adapter.SelectCommand = new MySqlCommand(commandText);
            Adapter.SelectCommand.CommandType = commandType;
            return this;
        }

        /// <summary>
        /// Creates a select command with the given stored procedure name and sets the table column mapping.
        /// </summary>
        public MySqlFluidAdapter CreateSelectForProcedure(string procedureName, string datasetTableName, List<string> selectColumns = null)
        {
            return CreateSelect(procedureName, datasetTableName, selectColumns, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public MySqlFluidAdapter Fragment(string fragmentValue)
        {
            if (String.IsNullOrEmpty(Adapter.SelectCommand.CommandText)) return this;
            Adapter.SelectCommand.CommandText += " " + fragmentValue;
            return this;
        }

        /// <summary>
        /// Adds a query fragment to a destination point in the select command.
        /// </summary>
        public MySqlFluidAdapter ComplexFragment(string fragmentName, string fragmentValue)
        {
            if (String.IsNullOrEmpty(Adapter.SelectCommand.CommandText)) return this;

            Adapter.SelectCommand.CommandText = Adapter.SelectCommand.CommandText
                .Replace("{" + fragmentName + "}", " " + fragmentValue + "{" + fragmentName + "}");

            return this;
        }

        /// <summary>
        /// Removes all fragment destinations from the select command, making the query ready to be executed.
        /// </summary>
        public MySqlFluidAdapter ComplexFragmentCleanUp()
        {
            string source = Adapter.SelectCommand.CommandText;
            if (String.IsNullOrEmpty(source)) return this;

            StringBuilder query = new StringBuilder();
            bool writeMode = true;
            for (int index = 0; index < source.Length; index++)
            {
                if (source[index] == '{')
                {
                    writeMode = false;
                    query.Append(" ");
                }
                else if (source[index] == '}')
                {
                    writeMode = true;
                }
                else if (writeMode)
                {
                    query.Append(source[index]);
                }
            }
            Adapter.SelectCommand.CommandText = query.ToString();
            return this;
        }

        /// <summary>
        /// Adds a MySqlParameter to the select command.
        /// </summary>
        public MySqlFluidAdapter SetParameter(string parameter, Type type)
        {
            if (TypeHinting.ContainsKey(type))
            {
                Hint hint = TypeHinting[type];
                Adapter.SelectCommand.Parameters.Add(new MySqlParameter(parameter, hint.MySqlDbType, hint.Size));
                return this;
            }
            throw new Exception("Not supported.");
        }

        /// <summary>
        /// Adds a MySqlParameter to the select command.
        /// </summary>
        public MySqlFluidAdapter SetParameter(string parameter, Hint hint)
        {
            Adapter.SelectCommand.Parameters.Add(new MySqlParameter(parameter, hint.MySqlDbType, hint.Size));
            return this;
        }

        /// <summary>
        /// Adds a field hint.
        /// </summary>
        public MySqlFluidAdapter AddFieldHint(string field, MySqlDbType type, int size)
        {
            if (FieldHinting.ContainsKey(field))
            {
                FieldHinting[field] = new Hint(type, size);
            }
            else
            {
                FieldHinting.Add(field, new Hint(type, size));
            }
            return this;
        }

        /// <summary>
        /// Adds the query condition to the select command.
        /// </summary>
        public MySqlFluidAdapter SetCondition(string condition, string connector = "AND")
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + condition;
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the MySqlParameter to the select command.
        /// </summary>
        public MySqlFluidAdapter SetCondition(string field, string parameter, Type type, string connector = "AND", string comparison = "=", string tableAlias = null)
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + (tableAlias != null ? tableAlias + "." : "") +
                                                 "`" + field + "` " + comparison + " " + parameter;

            if (type != null)
            {
                SetParameter(parameter, type);
            }
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the MySqlParameter to the select command.
        /// </summary>
        public MySqlFluidAdapter SetCondition(string field, string parameter, Hint hint, string connector = "AND", string comparison = "=", string tableAlias = null)
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + (tableAlias != null ? tableAlias + "." : "") +
                                                 "`" + field + "` " + comparison + " " + parameter;

            if (hint == null)
            {
                if (FieldHinting.ContainsKey(field))
                {
                    SetParameter(parameter, FieldHinting[field]);
                }
            }
            else
            {
                SetParameter(parameter, hint);
            }
            return this;
        }

        /// <summary>
        /// Creates all the commands for the adapter and sets the table column mapping.
        /// </summary>
        public MySqlFluidAdapter CreateUpdate(DataSet dataset, string datasetTableName, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null)
        {
            if (String.IsNullOrEmpty(databaseTableName)) databaseTableName = datasetTableName;
            Adapter.SelectCommand = CreateSelectCommand(dataset, datasetTableName, databaseTableName, selectColumns);
            Adapter.InsertCommand = CreateInsertCommand(dataset, datasetTableName, databaseTableName, selectColumns);
            Adapter.UpdateCommand = CreateUpdateCommand(dataset, datasetTableName, databaseTableName, selectColumns, whereColumns);
            Adapter.DeleteCommand = CreateDeleteCommand(dataset, datasetTableName, databaseTableName, whereColumns);
            SetMapping(dataset, datasetTableName);
            return this;
        }

        /// <summary>
        /// Creates all the commands for the adapter and sets the table column mapping.
        /// </summary>
        public MySqlFluidAdapter CreateUpdate(DataTable table, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null)
        {
            return CreateUpdate(table.DataSet, table.TableName, databaseTableName, selectColumns, whereColumns);
        }

        /// <summary>
        /// Creates a select command with the given verbatim query.
        /// </summary>
        public MySqlFluidAdapter CreateExecute(string query)
        {
            Adapter.SelectCommand = new MySqlCommand(query);
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using FluidFramework.Utilities;

namespace FluidFramework.SQLite.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate a SQLiteDataAdapter.
    /// </summary>
    public class SqliteFluidAdapter
    {
        /// <summary>
        /// The dynamically generated SQLiteDataAdapter.
        /// </summary>
        public SQLiteDataAdapter Adapter { get; set; }

        /// <summary>
        /// Marks the start of the WHERE part of the SELECT statement.
        /// </summary>
        public bool ConditionStart { get; set; }

        /// <summary>
        /// Helper property to create a new FluidAdapter instance.
        /// </summary>
        public static SqliteFluidAdapter New
        {
            get
            {
                return new SqliteFluidAdapter();
            }
        }

        /// <summary>
        /// A class that encapsulates a SQL type.
        /// </summary>
        public class Hint
        {
            /// <summary>
            /// Sql DbType
            /// </summary>
            public DbType DbType { get; set; }

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
            public Hint(DbType type, bool isdefault = false)
            {
                DbType = type;
                IsDefault = isdefault;
            }
        }

        /// <summary>
        /// The mapping between .NET types and SQLite types
        /// </summary>
        public Dictionary<Type, Hint> TypeHinting { get; set; }

        /// <summary>
        /// The mapping between fields and SQLite types
        /// </summary>
        public Dictionary<string, Hint> FieldHinting { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SqliteFluidAdapter()
        {
            Adapter = new SQLiteDataAdapter();
            ConditionStart = true;
            CreateTypeHints();
            FieldHinting = new Dictionary<string, Hint>();
        }

        #region Private Methods

        private void CreateTypeHints()
        {
            TypeHinting = new Dictionary<Type, Hint>
            {
                {typeof(String), new Hint(DbType.String, true)},
                {typeof(Byte), new Hint(DbType.Byte)},
                {typeof(Int16), new Hint(DbType.Int16)},
                {typeof(Int32), new Hint(DbType.Int32)},
                {typeof(Int64), new Hint(DbType.Int64)},
                {typeof(DateTime), new Hint(DbType.DateTime)},                
                {typeof(Decimal), new Hint(DbType.Decimal)},
                {typeof(Boolean), new Hint(DbType.Boolean)}
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

        private DbType GetDbType(DataColumn column)
        {
            if (FieldHinting.ContainsKey(column.ColumnName))
            {
                return FieldHinting[column.ColumnName].DbType;
            }
            if (TypeHinting.ContainsKey(column.DataType))
            {
                return TypeHinting[column.DataType].DbType;
            }
            return TypeHinting.FirstOrDefault(t => t.Value.IsDefault).Value.DbType;
        }

        private SQLiteParameter CreateParameter(DataColumn column)
        {
            return new SQLiteParameter("@" + SanitizeName(column.ColumnName), GetDbType(column), 0, column.ColumnName);
        }

        private SQLiteParameter CreateInputOriginalParameter(DataColumn column)
        {
            return new SQLiteParameter("@Original_" + SanitizeName(column.ColumnName), GetDbType(column), 0, ParameterDirection.Input, false, 0, 0, column.ColumnName, DataRowVersion.Original, null);
        }

        private string ExtraSelectText(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, bool isInsert)
        {
            try
            {
                if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
                DataColumn pkColumn = dataset.Tables[datasetTableName].PrimaryKey.FirstOrDefault();
                if (pkColumn == null) return "";
                string commandText = ";SELECT " + GenericTools.Enumerate(selectColumns, ", ", "\"", "\"") + " FROM \"" + databaseTableName + "\" WHERE ";
                if (isInsert && pkColumn.AutoIncrement)
                {
                    commandText += "(\"" + pkColumn.ColumnName + "\" = LAST_INSERT_ROWID())";
                }
                else
                {
                    commandText += "(\"" + pkColumn.ColumnName + "\" = @" + SanitizeName(pkColumn.ColumnName) + ")";
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
                string line = "(\"" + column.ColumnName + "\" = @Original_" + SanitizeName(column.ColumnName) + ")";
                if (column.AllowDBNull)
                {
                    line = "((@Original_" + SanitizeName(column.ColumnName) + " IS NULL AND \"" + column.ColumnName + "\" IS NULL) OR " + line + ")";
                }
                result.Append(line);
            }
            return result.ToString();
        }

        private SQLiteCommand CreateSelectCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            SQLiteCommand command = new SQLiteCommand
            {
                CommandText = "SELECT " + GenericTools.Enumerate(selectColumns, ", ", "\"", "\"") + " FROM \"" + databaseTableName + "\"",
                CommandType = CommandType.Text
            };
            return command;
        }

        private SQLiteCommand CreateInsertCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            List<string> valueColumns = (from string column in selectColumns
                where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                select SanitizeName(column)).ToList();
            List<string> insertColumns = (from string column in selectColumns
                where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                select column).ToList();
            SQLiteCommand command = new SQLiteCommand
            {
                CommandText = "INSERT INTO \"" + databaseTableName + "\" (" + GenericTools.Enumerate(insertColumns, ", ", "\"", "\"") + ") " +
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

        private SQLiteCommand CreateUpdateCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, List<string> whereColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            SQLiteCommand command = new SQLiteCommand
            {
                CommandText = "UPDATE \"" + databaseTableName + "\" SET " +
                              GenericTools.Enumerate((from string column in selectColumns
                                  where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                  select "\"" + column + "\" = @" + SanitizeName(column)).ToList(), ", ") +
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

        private SQLiteCommand CreateDeleteCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> whereColumns)
        {
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            SQLiteCommand command = new SQLiteCommand
            {
                CommandText = "DELETE FROM \"" + databaseTableName + "\" WHERE (" + UpdateDeleteWhereText(dataset, datasetTableName, whereColumns) + ")",
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
        public SqliteFluidAdapter CreateSelect(string databaseTableName, List<string> selectColumns, string tableAlias = null, string datasetTableName = null)
        {
            ConditionStart = true;
            if (datasetTableName == null) datasetTableName = databaseTableName;
            SetMapping(datasetTableName, selectColumns);
            Adapter.SelectCommand = new SQLiteCommand("SELECT " + (selectColumns == null || selectColumns.Count < 1 ? (tableAlias != null ? tableAlias + "." : "") + "*" :
                                                      GenericTools.Enumerate(selectColumns, ", ", (tableAlias != null ? tableAlias + "." : "") + "\"", "\"")) +
                                                      " FROM \"" + databaseTableName + "\"" + (tableAlias != null ? " AS " + tableAlias : ""));
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        /// <summary>
        /// Creates a select command and sets the table column mapping.
        /// </summary>
        public SqliteFluidAdapter CreateSelect(DataTable table, List<string> selectColumns = null, string tableAlias = null, string databaseTableName = null)
        {
            if (selectColumns == null) selectColumns = GenericTools.AllColumns(table);
            if (databaseTableName == null) databaseTableName = table.TableName;
            return CreateSelect(databaseTableName, selectColumns, tableAlias, table.TableName);
        }

        /// <summary>
        /// Creates a select command with the given verbatim command text and sets the table column mapping.
        /// </summary>
        public SqliteFluidAdapter CreateSelect(string commandText, string datasetTableName, List<string> selectColumns = null)
        {
            ConditionStart = true;
            SetMapping(datasetTableName, selectColumns);
            Adapter.SelectCommand = new SQLiteCommand(commandText);
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public SqliteFluidAdapter Fragment(string fragmentValue)
        {
            if (String.IsNullOrEmpty(Adapter.SelectCommand.CommandText)) return this;
            Adapter.SelectCommand.CommandText += " " + fragmentValue;
            return this;
        }

        /// <summary>
        /// Adds a query fragment to a destination point in the select command.
        /// </summary>
        public SqliteFluidAdapter ComplexFragment(string fragmentName, string fragmentValue)
        {
            if (String.IsNullOrEmpty(Adapter.SelectCommand.CommandText)) return this;

            Adapter.SelectCommand.CommandText = Adapter.SelectCommand.CommandText
                .Replace("{" + fragmentName + "}", " " + fragmentValue + "{" + fragmentName + "}");

            return this;
        }

        /// <summary>
        /// Removes all fragment destinations from the select command, making the query ready to be executed.
        /// </summary>
        public SqliteFluidAdapter ComplexFragmentCleanUp()
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
        /// Adds a SQLiteParameter to the select command.
        /// </summary>
        public SqliteFluidAdapter SetParameter(string parameter, Type type)
        {
            if (TypeHinting.ContainsKey(type))
            {
                Hint hint = TypeHinting[type];
                Adapter.SelectCommand.Parameters.Add(new SQLiteParameter(parameter, hint.DbType));
                return this;
            }
            throw new Exception("Not supported.");
        }

        /// <summary>
        /// Adds a SQLiteParameter to the select command.
        /// </summary>
        public SqliteFluidAdapter SetParameter(string parameter, Hint hint)
        {
            Adapter.SelectCommand.Parameters.Add(new SQLiteParameter(parameter, hint.DbType));
            return this;
        }

        /// <summary>
        /// Adds a field hint.
        /// </summary>
        public SqliteFluidAdapter AddFieldHint(string field, DbType type)
        {
            if (FieldHinting.ContainsKey(field))
            {
                FieldHinting[field] = new Hint(type);
            }
            else
            {
                FieldHinting.Add(field, new Hint(type));
            }
            return this;
        }

        /// <summary>
        /// Adds the query condition to the select command.
        /// </summary>
        public SqliteFluidAdapter SetCondition(string condition, string connector = "AND")
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + condition;
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the SQLiteParameter to the select command.
        /// </summary>
        public SqliteFluidAdapter SetCondition(string field, string parameter, Type type, string connector = "AND", string comparison = "=", string tableAlias = null)
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + (tableAlias != null ? tableAlias + "." : "") +
                                                 "\"" + field + "\" " + comparison + " " + parameter;

            if (type != null)
            {
                SetParameter(parameter, type);
            }
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the SQLiteParameter to the select command.
        /// </summary>
        public SqliteFluidAdapter SetCondition(string field, string parameter, Hint hint, string connector = "AND", string comparison = "=", string tableAlias = null)
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + (tableAlias != null ? tableAlias + "." : "") +
                                                 "\"" + field + "\" " + comparison + " " + parameter;

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
        public SqliteFluidAdapter CreateUpdate(DataSet dataset, string datasetTableName, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null)
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
        public SqliteFluidAdapter CreateUpdate(DataTable table, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null)
        {
            return CreateUpdate(table.DataSet, table.TableName, databaseTableName, selectColumns, whereColumns);
        }

        /// <summary>
        /// Creates a select command with the given verbatim query.
        /// </summary>
        public SqliteFluidAdapter CreateExecute(string query)
        {
            Adapter.SelectCommand = new SQLiteCommand(query);
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        #endregion
    }
}

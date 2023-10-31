using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FluidFramework.Utilities;

namespace FluidFramework.SqlServer.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate an SqlDataAdapter.
    /// </summary>
    public class SqlServerFluidAdapter
    {
        /// <summary>
        /// The dynamically generated SqlDataAdapter.
        /// </summary>
        public SqlDataAdapter Adapter { get; set; }

        /// <summary>
        /// Marks the start of the WHERE part of the SELECT statement.
        /// </summary>
        public bool ConditionStart { get; set; }

        /// <summary>
        /// Helper method to create a new FluidAdapter instance.
        /// </summary>
        public static SqlServerFluidAdapter New
        {
            get
            {
                return new SqlServerFluidAdapter();
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
            public SqlDbType SqlDbType { get; set; }

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
            public Hint(){}

            /// <summary>
            /// Constructor with parameters
            /// </summary>
            public Hint(SqlDbType type, int size, bool isdefault = false)
            {
                SqlDbType = type;
                Size = size;
                IsDefault = isdefault;
            }
        }

        /// <summary>
        /// The mapping between .NET types and SQL types
        /// </summary>
        public Dictionary<Type, Hint> TypeHinting { get; set; }

        /// <summary>
        /// The mapping between fields and SQL types
        /// </summary>
        public Dictionary<string, Hint> FieldHinting { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlServerFluidAdapter()
        {
            Adapter = new SqlDataAdapter();
            ConditionStart = true;
            CreateTypeHints();
            FieldHinting = new Dictionary<string, Hint>();
        }

        #region Private Methods

        private void CreateTypeHints()
        {
            TypeHinting = new Dictionary<Type, Hint>
            {
                {typeof(String), new Hint(SqlDbType.NVarChar, -1, true)},
                {typeof(Guid), new Hint(SqlDbType.UniqueIdentifier, 16)},
                {typeof(Int32), new Hint(SqlDbType.Int, 4)},
                {typeof(Boolean), new Hint(SqlDbType.Bit, 1)},
                {typeof(DateTime), new Hint(SqlDbType.DateTime, 8)},
                {typeof(Byte), new Hint(SqlDbType.TinyInt, 1)},
                {typeof(Int16), new Hint(SqlDbType.SmallInt, 2)},
                {typeof(Int64), new Hint(SqlDbType.BigInt, 8)},
                {typeof(Decimal), new Hint(SqlDbType.Decimal, 9)}
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

        private SqlDbType GetDbType(DataColumn column)
        {
            if (FieldHinting.ContainsKey(column.ColumnName))
            {
                return FieldHinting[column.ColumnName].SqlDbType;
            }
            if (TypeHinting.ContainsKey(column.DataType))
            {
                return TypeHinting[column.DataType].SqlDbType;
            }
            return TypeHinting.FirstOrDefault(t => t.Value.IsDefault).Value.SqlDbType;
        }

        private SqlParameter CreateParameter(DataColumn column)
        {
            return new SqlParameter("@" + SanitizeName(column.ColumnName), GetDbType(column), 0, column.ColumnName);
        }

        private SqlParameter CreateInputOriginalParameter(DataColumn column)
        {
            return new SqlParameter("@Original_" + SanitizeName(column.ColumnName), GetDbType(column), 0, ParameterDirection.Input, false, 0, 0, column.ColumnName, DataRowVersion.Original, null);
        }

        private SqlParameter CreateIsNullParameter(DataColumn column)
        {
            return new SqlParameter("@IsNull_" + SanitizeName(column.ColumnName), SqlDbType.Int, 0, ParameterDirection.Input, 0, 0, column.ColumnName, DataRowVersion.Original, true, null, "", "", "");
        }

        private string ExtraSelectText(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, string databaseSchema, bool isInsert)
        {
            try
            {
                if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
                DataColumn pkColumn = dataset.Tables[datasetTableName].PrimaryKey.FirstOrDefault();
                if (pkColumn == null) return "";
                string commandText = ";SELECT " + GenericTools.Enumerate(selectColumns, ", ", "[", "]") + " FROM " +
                                     (String.IsNullOrEmpty(databaseSchema) ? "" : "[" + databaseSchema + "].") + "[" + databaseTableName + "] WHERE ";
                if (isInsert && pkColumn.AutoIncrement)
                {
                    commandText += "([" + pkColumn.ColumnName + "] = SCOPE_IDENTITY())";
                }
                else
                {
                    commandText += "([" + pkColumn.ColumnName + "] = @" + SanitizeName(pkColumn.ColumnName) + ")";
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
                string line = "([" + column.ColumnName + "] = @Original_" + SanitizeName(column.ColumnName) + ")";
                if (column.AllowDBNull)
                {
                    line = "((@IsNull_" + SanitizeName(column.ColumnName) + " = 1 AND [" + column.ColumnName + "] IS NULL) OR " + line + ")";
                }
                result.Append(line);
            }
            return result.ToString();
        }

        private SqlCommand CreateSelectCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, string databaseSchema)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            SqlCommand command = new SqlCommand
            {
                CommandText = "SELECT " + GenericTools.Enumerate(selectColumns, ", ", "[", "]") + " FROM " +
                              (String.IsNullOrEmpty(databaseSchema) ? "" : "[" + databaseSchema + "].") + "[" + databaseTableName + "]",
                CommandType = CommandType.Text
            };
            return command;
        }

        private SqlCommand CreateInsertCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, string databaseSchema)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            List<string> valueColumns = (from string column in selectColumns
                                         where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                         select SanitizeName(column)).ToList();
            List<string> insertColumns = (from string column in selectColumns
                                          where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                          select column).ToList();
            SqlCommand command = new SqlCommand
            {
                CommandText = "INSERT INTO " + (String.IsNullOrEmpty(databaseSchema) ? "" : "[" + databaseSchema + "].") + "[" + databaseTableName + "] (" +
                              GenericTools.Enumerate(insertColumns, ", ", "[", "]") + ") " + "VALUES (" + GenericTools.Enumerate(valueColumns, ", ", "@") + ")",
                CommandType = CommandType.Text
            };

            command.CommandText += ExtraSelectText(dataset, datasetTableName, databaseTableName, selectColumns, databaseSchema, true);

            foreach (string columnName in insertColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateParameter(column));
            }
            return command;
        }

        private SqlCommand CreateUpdateCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, List<string> whereColumns, string databaseSchema)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            SqlCommand command = new SqlCommand
            {
                CommandText = "UPDATE " + (String.IsNullOrEmpty(databaseSchema) ? "" : "[" + databaseSchema + "].") + "[" + databaseTableName + "] SET " +
                              GenericTools.Enumerate((from string column in selectColumns
                                                      where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                                      select "[" + column + "] = @" + SanitizeName(column)).ToList(), ", ") +
                              " WHERE (" + UpdateDeleteWhereText(dataset, datasetTableName, whereColumns) + ")",
                CommandType = CommandType.Text
            };

            command.CommandText += ExtraSelectText(dataset, datasetTableName, databaseTableName, selectColumns, databaseSchema, false);

            foreach (string columnName in (from string column in selectColumns where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false select column).ToList())
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateParameter(column));
            }

            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateInputOriginalParameter(column));
                if (column.AllowDBNull)
                {
                    command.Parameters.Add(CreateIsNullParameter(column));
                }
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

        private SqlCommand CreateDeleteCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> whereColumns, string databaseSchema)
        {
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            SqlCommand command = new SqlCommand
            {
                CommandText = "DELETE FROM " + (String.IsNullOrEmpty(databaseSchema) ? "" : "[" + databaseSchema + "].") + "[" + databaseTableName + "] WHERE (" +
                              UpdateDeleteWhereText(dataset, datasetTableName, whereColumns) + ")",
                CommandType = CommandType.Text
            };

            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateInputOriginalParameter(column));
                if (column.AllowDBNull)
                {
                    command.Parameters.Add(CreateIsNullParameter(column));
                }
            }
            return command;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a select command and sets the table column mapping.
        /// </summary>
        public SqlServerFluidAdapter CreateSelect(string databaseTableName, List<string> selectColumns, string tableAlias = null, string datasetTableName = null, string databaseSchema = null)
        {
            ConditionStart = true;
            if (datasetTableName == null) datasetTableName = databaseTableName;
            SetMapping(datasetTableName, selectColumns);            
            Adapter.SelectCommand = new SqlCommand("SELECT " + (selectColumns == null || selectColumns.Count < 1 ? (tableAlias != null ? tableAlias + "." : "") + "*" :
                                                                GenericTools.Enumerate(selectColumns, ", ", (tableAlias != null ? tableAlias + "." : "") + "[", "]")) +
                                                   " FROM " + (String.IsNullOrEmpty(databaseSchema) ? "" : "[" + databaseSchema + "].") + "[" + databaseTableName + "]" + (tableAlias != null ? " AS " + tableAlias : ""));
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        /// <summary>
        /// Creates a select command and sets the table column mapping.
        /// </summary>
        public SqlServerFluidAdapter CreateSelect(DataTable table, List<string> selectColumns = null, string tableAlias = null, string databaseTableName = null, string databaseSchema = null)
        {
            if (selectColumns == null) selectColumns = GenericTools.AllColumns(table);
            if (databaseTableName == null) databaseTableName = table.TableName;
            return CreateSelect(databaseTableName, selectColumns, tableAlias, table.TableName, databaseSchema);
        }

        /// <summary>
        /// Creates a select command with the given verbatim command text and sets the table column mapping.
        /// </summary>
        public SqlServerFluidAdapter CreateSelect(string commandText, string datasetTableName, List<string> selectColumns = null, CommandType commandType = CommandType.Text)
        {
            ConditionStart = true;
            SetMapping(datasetTableName, selectColumns);
            Adapter.SelectCommand = new SqlCommand(commandText);
            Adapter.SelectCommand.CommandType = commandType;
            return this;
        }

        /// <summary>
        /// Creates a select command with the given stored procedure name and sets the table column mapping.
        /// </summary>
        public SqlServerFluidAdapter CreateSelectForProcedure(string procedureName, string datasetTableName, List<string> selectColumns = null)
        {
            return CreateSelect(procedureName, datasetTableName, selectColumns, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public SqlServerFluidAdapter Fragment(string fragment)
        {
            Adapter.SelectCommand.CommandText += " " + fragment;
            return this;
        }

        /// <summary>
        /// Adds a SqlParameter to the select command.
        /// </summary>
        public SqlServerFluidAdapter SetParameter(string parameter, Type type)
        {
            if (TypeHinting.ContainsKey(type))
            {
                Hint hint = TypeHinting[type];
                Adapter.SelectCommand.Parameters.Add(new SqlParameter(parameter, hint.SqlDbType, hint.Size));
                return this;
            }            
            throw new Exception("Not supported.");
        }

        /// <summary>
        /// Adds a SqlParameter to the select command.
        /// </summary>
        public SqlServerFluidAdapter SetParameter(string parameter, Hint hint)
        {
            Adapter.SelectCommand.Parameters.Add(new SqlParameter(parameter, hint.SqlDbType, hint.Size));
            return this;
        }

        /// <summary>
        /// Adds a field hint.
        /// </summary>
        public SqlServerFluidAdapter AddFieldHint(string field, SqlDbType type, int size)
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
        public SqlServerFluidAdapter SetCondition(string condition, string connector = "AND")
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + condition;
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the SqlParameter to the select command.
        /// </summary>
        public SqlServerFluidAdapter SetCondition(string field, string parameter, Type type, string connector = "AND", string comparison = "=", string tableAlias = null)
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + (tableAlias != null ? tableAlias + "." : "") +
                                                 "[" + field + "] " + comparison + " " + parameter;

            if (type != null)
            {
                SetParameter(parameter, type);
            }
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the SqlParameter to the select command.
        /// </summary>
        public SqlServerFluidAdapter SetCondition(string field, string parameter, Hint hint, string connector = "AND", string comparison = "=", string tableAlias = null)
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + (tableAlias != null ? tableAlias + "." : "") +
                                                 "[" + field + "] " + comparison + " " + parameter;

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
        public SqlServerFluidAdapter CreateUpdate(DataSet dataset, string datasetTableName, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null, string databaseSchema = null)
        {
            if (String.IsNullOrEmpty(databaseTableName)) databaseTableName = datasetTableName;
            Adapter.SelectCommand = CreateSelectCommand(dataset, datasetTableName, databaseTableName, selectColumns, databaseSchema);
            Adapter.InsertCommand = CreateInsertCommand(dataset, datasetTableName, databaseTableName, selectColumns, databaseSchema);
            Adapter.UpdateCommand = CreateUpdateCommand(dataset, datasetTableName, databaseTableName, selectColumns, whereColumns, databaseSchema);
            Adapter.DeleteCommand = CreateDeleteCommand(dataset, datasetTableName, databaseTableName, whereColumns, databaseSchema);
            SetMapping(dataset, datasetTableName);
            return this;
        }

        /// <summary>
        /// Creates all the commands for the adapter and sets the table column mapping.
        /// </summary>
        public SqlServerFluidAdapter CreateUpdate(DataTable table, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null, string databaseSchema = null)
        {
            return CreateUpdate(table.DataSet, table.TableName, databaseTableName, selectColumns, whereColumns, databaseSchema);
        }

        /// <summary>
        /// Creates a select command with the given verbatim query.
        /// </summary>
        public SqlServerFluidAdapter CreateExecute(string query)
        {
            Adapter.SelectCommand = new SqlCommand(query);
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        #endregion
    }
}

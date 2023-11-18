using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluidFramework.Utilities;
using Oracle.ManagedDataAccess.Client;

namespace FluidFramework.Oracle.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate an OracleDataAdapter.
    /// </summary>
    public class OracleFluidAdapter
    {
        /// <summary>
        /// The dynamically generated OracleDataAdapter.
        /// </summary>
        public OracleDataAdapter Adapter { get; set; }

        /// <summary>
        /// Marks the start of the WHERE part of the SELECT statement.
        /// </summary>
        public bool ConditionStart { get; set; }

        /// <summary>
        /// Helper property to create a new FluidAdapter instance.
        /// </summary>
        public static OracleFluidAdapter New
        {
            get
            {
                return new OracleFluidAdapter();
            }
        }

        /// <summary>
        /// A class that encapsulates an Oracle type.
        /// </summary>
        public class Hint
        {
            /// <summary>
            /// SqlDbType
            /// </summary>
            public OracleDbType OracleDbType { get; set; }

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
            public Hint(OracleDbType type, bool isdefault = false)
            {
                OracleDbType = type;
                IsDefault = isdefault;
            }
        }

        /// <summary>
        /// The mapping between .NET types and Oracle types
        /// </summary>
        public Dictionary<Type, Hint> TypeHinting { get; set; }

        /// <summary>
        /// The mapping between fields and Oracle types
        /// </summary>
        public Dictionary<string, Hint> FieldHinting { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OracleFluidAdapter()
        {
            Adapter = new OracleDataAdapter();
            ConditionStart = true;
            CreateTypeHints();
            FieldHinting = new Dictionary<string, Hint>();
        }

        #region Private Methods

        private void CreateTypeHints()
        {
            TypeHinting = new Dictionary<Type, Hint>
            {
                {typeof(String), new Hint(OracleDbType.NVarchar2, true)},
                {typeof(DateTime), new Hint(OracleDbType.Date)},
                {typeof(Int16), new Hint(OracleDbType.Decimal)},
                {typeof(Int32), new Hint(OracleDbType.Decimal)},
                {typeof(Int64), new Hint(OracleDbType.Decimal)},
                {typeof(Decimal), new Hint(OracleDbType.Decimal)}                
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
            return name.Replace(" ", "_").ToUpper();
        }

        private OracleDbType GetDbType(DataColumn column)
        {
            if (FieldHinting.ContainsKey(column.ColumnName))
            {
                return FieldHinting[column.ColumnName].OracleDbType;
            }
            if (TypeHinting.ContainsKey(column.DataType))
            {
                return TypeHinting[column.DataType].OracleDbType;
            }
            return TypeHinting.FirstOrDefault(t => t.Value.IsDefault).Value.OracleDbType;
        }

        private OracleParameter CreateParameter(DataColumn column)
        {
            OracleParameter parameter = new OracleParameter
            {
                ParameterName = ":cur_" + SanitizeName(column.ColumnName),
                OracleDbType = GetDbType(column),
                SourceColumn = column.ColumnName
            };
            return parameter;
        }

        private OracleParameter CreateInputOriginalParameter(DataColumn column, bool usedForNullTest)
        {
            OracleParameter parameter = new OracleParameter
            {
                ParameterName = ":ori_" + SanitizeName(column.ColumnName) + (usedForNullTest ? "_n" : "_p"),
                OracleDbType = GetDbType(column),
                SourceColumn = column.ColumnName,
                SourceVersion = DataRowVersion.Original
            };
            return parameter;
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
                string line = "\"" + column.ColumnName + "\"=:ori_" + SanitizeName(column.ColumnName) + "_p";
                if (column.AllowDBNull)
                {
                    line = "((:ori_" + SanitizeName(column.ColumnName) + "_n IS NULL AND \"" + column.ColumnName + "\" IS NULL) OR " + line + ")";
                }
                result.Append(line);
            }
            return result.ToString();
        }

        private OracleCommand CreateSelectCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            OracleCommand command = new OracleCommand
            {
                CommandText = "SELECT " + GenericTools.Enumerate(selectColumns, ", ", "\"", "\"") + " FROM \"" + databaseTableName + "\"",
                CommandType = CommandType.Text
            };
            return command;
        }

        private OracleCommand CreateInsertCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            List<string> valueColumns = (from string column in selectColumns
                                         where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                         select SanitizeName(column)).ToList();
            List<string> insertColumns = (from string column in selectColumns
                                          where dataset.Tables[datasetTableName].Columns[column].AutoIncrement == false
                                          select column).ToList();
            OracleCommand command = new OracleCommand
            {
                CommandText = "INSERT INTO \"" + databaseTableName + "\" (" + GenericTools.Enumerate(insertColumns, ", ", "\"", "\"") + ") " +
                              "VALUES (" + GenericTools.Enumerate(valueColumns, ", ", ":cur_") + ")",
                CommandType = CommandType.Text
            };

            foreach (string columnName in insertColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateParameter(column));
            }
            return command;
        }

        private OracleCommand CreateUpdateCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> selectColumns, List<string> whereColumns)
        {
            if (selectColumns == null) selectColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            OracleCommand command = new OracleCommand
            {
                CommandText = "UPDATE \"" + databaseTableName + "\" SET " +
                    GenericTools.Enumerate((from string column in selectColumns select "\"" + column + "\"=:cur_" + SanitizeName(column)).ToList(), ", ") +
                    " WHERE " + UpdateDeleteWhereText(dataset, datasetTableName, whereColumns),
                CommandType = CommandType.Text
            };

            foreach (string columnName in (from string column in selectColumns select column).ToList())
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                command.Parameters.Add(CreateParameter(column));
            }

            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                if (column.AllowDBNull)
                {
                    command.Parameters.Add(CreateInputOriginalParameter(column, true));
                }
                command.Parameters.Add(CreateInputOriginalParameter(column, false));
            }
            return command;
        }

        private OracleCommand CreateDeleteCommand(DataSet dataset, string datasetTableName, string databaseTableName, List<string> whereColumns)
        {
            if (whereColumns == null) whereColumns = (from DataColumn column in dataset.Tables[datasetTableName].Columns select column.ColumnName).ToList();
            OracleCommand command = new OracleCommand
            {
                CommandText = "DELETE FROM \"" + databaseTableName + "\" WHERE " + UpdateDeleteWhereText(dataset, datasetTableName, whereColumns),
                CommandType = CommandType.Text
            };

            foreach (string columnName in whereColumns)
            {
                DataColumn column = dataset.Tables[datasetTableName].Columns[columnName];
                if (column.AllowDBNull)
                {
                    command.Parameters.Add(CreateInputOriginalParameter(column, true));
                }
                command.Parameters.Add(CreateInputOriginalParameter(column, false));
            }
            return command;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a select command.
        /// </summary>
        public OracleFluidAdapter CreateSelect(string databaseTableName, List<string> selectColumns, string tableAlias = null)
        {
            ConditionStart = true;
            Adapter.SelectCommand = new OracleCommand("SELECT " + (selectColumns == null || selectColumns.Count < 1 ? (tableAlias != null ? tableAlias + "." : "") + "*" :
                                                      GenericTools.Enumerate(selectColumns, ", ", (tableAlias != null ? tableAlias + "." : "") + "\"", "\"")) +
                                                      " FROM \"" + databaseTableName + "\"" + (tableAlias == null ? "" : " " + tableAlias));
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        /// <summary>
        /// Creates a select command.
        /// </summary>
        public OracleFluidAdapter CreateSelect(DataTable table, List<string> selectColumns = null, string tableAlias = null, string databaseTableName = null)
        {
            if (selectColumns == null) selectColumns = GenericTools.AllColumns(table);
            if (databaseTableName == null) databaseTableName = table.TableName;
            return CreateSelect(databaseTableName, selectColumns, tableAlias);
        }

        /// <summary>
        /// Creates a select command with the given verbatim command text.
        /// </summary>
        public OracleFluidAdapter CreateSelect(string commandText, CommandType commandType = CommandType.Text)
        {
            ConditionStart = true;
            Adapter.SelectCommand = new OracleCommand(commandText);
            Adapter.SelectCommand.CommandType = commandType;
            return this;
        }

        /// <summary>
        /// Creates a select command with the given stored procedure name.
        /// </summary>
        public OracleFluidAdapter CreateSelectForProcedure(string procedureName)
        {
            return CreateSelect(procedureName, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public OracleFluidAdapter Fragment(string fragment)
        {
            Adapter.SelectCommand.CommandText += " " + fragment;
            return this;
        }

        /// <summary>
        /// Adds an OracleParameter to the select command.
        /// </summary>
        public OracleFluidAdapter SetParameter(string parameter, Type type)
        {
            if (TypeHinting.ContainsKey(type))
            {
                Hint hint = TypeHinting[type];
                Adapter.SelectCommand.Parameters.Add(new OracleParameter(parameter, hint.OracleDbType));
                return this;
            }
            throw new Exception("Not supported.");
        }

        /// <summary>
        /// Adds an OracleParameter to the select command.
        /// </summary>
        public OracleFluidAdapter SetParameter(string parameter, Hint hint)
        {
            Adapter.SelectCommand.Parameters.Add(new OracleParameter(parameter, hint.OracleDbType));
            return this;
        }

        /// <summary>
        /// Adds a field hint.
        /// </summary>
        public OracleFluidAdapter AddFieldHint(string field, OracleDbType type)
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
        public OracleFluidAdapter SetCondition(string condition, string connector = "AND")
        {
            Adapter.SelectCommand.CommandText += GetConnector(connector) + condition;
            return this;
        }

        /// <summary>
        /// Adds the parameter condition and the OracleParameter to the select command.
        /// </summary>
        public OracleFluidAdapter SetCondition(string field, string parameter, Type type, string connector = "AND", string comparison = "=", string tableAlias = null)
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
        /// Adds the parameter condition and the OracleParameter to the select command.
        /// </summary>
        public OracleFluidAdapter SetCondition(string field, string parameter, Hint hint, string connector = "AND", string comparison = "=", string tableAlias = null)
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
        /// Creates all the commands for the adapter.
        /// </summary>
        public OracleFluidAdapter CreateUpdate(DataSet dataset, string datasetTableName, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null)
        {
            if (String.IsNullOrEmpty(databaseTableName)) databaseTableName = datasetTableName;
            Adapter.SelectCommand = CreateSelectCommand(dataset, datasetTableName, databaseTableName, selectColumns);
            Adapter.InsertCommand = CreateInsertCommand(dataset, datasetTableName, databaseTableName, selectColumns);
            Adapter.UpdateCommand = CreateUpdateCommand(dataset, datasetTableName, databaseTableName, selectColumns, whereColumns);
            Adapter.DeleteCommand = CreateDeleteCommand(dataset, datasetTableName, databaseTableName, whereColumns);
            return this;
        }

        /// <summary>
        /// Creates all the commands for the adapter.
        /// </summary>
        public OracleFluidAdapter CreateUpdate(DataTable table, string databaseTableName = null, List<string> selectColumns = null, List<string> whereColumns = null)
        {
            return CreateUpdate(table.DataSet, table.TableName, databaseTableName, selectColumns, whereColumns);
        }

        /// <summary>
        /// Creates a select command with the given verbatim query.
        /// </summary>
        public OracleFluidAdapter CreateExecute(string query)
        {
            Adapter.SelectCommand = new OracleCommand(query);
            Adapter.SelectCommand.CommandType = CommandType.Text;
            return this;
        }

        #endregion
    }
}

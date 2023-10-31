using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using FluidFramework.Data;

namespace FluidFramework.SQLite.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate an SqliteAdapterConfiguration.
    /// </summary>
    public class SqliteFluidSelector
    {
        /// <summary>
        /// The underlying fluid adapter.
        /// </summary>
        public SqliteFluidAdapter Adapter { get; set; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public List<ParameterInfo> Parameters { get; private set; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// The dynamically generated SQLiteDataAdapter.
        /// </summary>
        public SQLiteDataAdapter SqliteAdapter
        {
            get { return Adapter.Adapter; }
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with the underlying fluid adapter.
        /// </summary>
        public static SqliteFluidSelector New(SqliteFluidAdapter adapter)
        {
            return new SqliteFluidSelector(adapter);
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with a generic select query.
        /// </summary>
        public static SqliteFluidSelector New(string table, string query = null)
        {
            if (query == null) query = "SELECT * FROM \"" + table + "\"";
            return new SqliteFluidSelector(SqliteFluidAdapter.New.CreateSelect(query, table)) { Table = table };
        }

        /// <summary>
        /// Constructor that initializes the underlying fluid adapter.
        /// </summary>
        public SqliteFluidSelector(SqliteFluidAdapter adapter)
        {
            Adapter = adapter;
            Parameters = new List<ParameterInfo>();
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public SqliteFluidSelector SetParameter(string parameter, object value, DbType type, bool inject = true, string comparison = "=")
        {
            if (String.IsNullOrEmpty(parameter)) throw new Exception("Undefined parameter name.");
            string parameterName = "@" + Regex.Replace(parameter, "[^\\w\\._]", "");

            if (value == null) value = DBNull.Value;

            Parameters.Add(new ParameterInfo(parameterName, value));
            Adapter.SetParameter(parameterName, new SqliteFluidAdapter.Hint(type));
            if (inject)
            {
                Adapter.SetCondition("\"" + parameter + "\" " + comparison + " " + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public SqliteFluidSelector SetParameter(string parameter, object value, Type type, bool inject = true, string comparison = "=")
        {
            if (String.IsNullOrEmpty(parameter)) throw new Exception("Undefined parameter name.");
            if (value == null && type == null) throw new Exception("Undefined parameter type.");

            string parameterName = "@" + Regex.Replace(parameter, "[^\\w\\._]", "");
            Type parameterType = type ?? value.GetType();

            if (value == null) value = DBNull.Value;

            Parameters.Add(new ParameterInfo(parameterName, value));
            Adapter.SetParameter(parameterName, parameterType);
            if (inject)
            {
                Adapter.SetCondition("\"" + parameter + "\" " + comparison + " " + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public SqliteFluidSelector SetParameter(string parameter, object value, bool inject = true, string comparison = "=")
        {
            return SetParameter(parameter, value, null, inject, comparison);
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public SqliteFluidSelector Fragment(string fragment)
        {
            Adapter.Fragment(fragment);
            return this;
        }

        /// <summary>
        /// Adds the query condition to the select command.
        /// </summary>
        public SqliteFluidSelector SetCondition(string condition, string connector = "AND")
        {
            Adapter.SetCondition(condition, connector);
            return this;
        }

        /// <summary>
        /// Returns the dynamically generated SqliteAdapterConfiguration.
        /// </summary>
        public SqliteAdapterConfiguration Configuration(DataSet dataset, string table = null)
        {
            if (table == null) table = Table;
            return new SqliteAdapterConfiguration(dataset, table, SqliteAdapter, Parameters, SqlAction.Get);
        }
    }
}

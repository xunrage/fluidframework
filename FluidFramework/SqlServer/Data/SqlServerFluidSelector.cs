using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using FluidFramework.Data;

namespace FluidFramework.SqlServer.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate an SqlServerAdapterConfiguration.
    /// </summary>
    public class SqlServerFluidSelector
    {
        /// <summary>
        /// The underlying fluid adapter.
        /// </summary>
        public SqlServerFluidAdapter Adapter { get; set; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public List<ParameterInfo> Parameters { get; private set; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// The dynamically generated SqlDataAdapter.
        /// </summary>
        public SqlDataAdapter SqlAdapter
        {
            get { return Adapter.Adapter; }
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with the underlying fluid adapter.
        /// </summary>
        public static SqlServerFluidSelector New(SqlServerFluidAdapter adapter)
        {
            return new SqlServerFluidSelector(adapter);
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with a generic select query.
        /// </summary>
        public static SqlServerFluidSelector New(string table, string query = null, string schema = null)
        {
            if (query == null) query = "SELECT * FROM " + (String.IsNullOrEmpty(schema) ? "" : "[" + schema + "].") + "[" + table + "]";
            return new SqlServerFluidSelector(SqlServerFluidAdapter.New.CreateSelect(query, table)) { Table = table };
        }

        /// <summary>
        /// Constructor that initializes the underlying fluid adapter.
        /// </summary>
        public SqlServerFluidSelector(SqlServerFluidAdapter adapter)
        {
            Adapter = adapter;
            Parameters = new List<ParameterInfo>();
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public SqlServerFluidSelector SetParameter(string parameter, object value, SqlDbType type, int size, bool inject = true, string comparison = "=")
        {
            if (String.IsNullOrEmpty(parameter)) throw new Exception("Undefined parameter name.");
            string parameterName = "@" + Regex.Replace(parameter, "[^\\w\\._]", "");

            if (size == -1)
            {
                foreach (KeyValuePair<Type, SqlServerFluidAdapter.Hint> hint in Adapter.TypeHinting)
                {
                    if (hint.Value.SqlDbType == type)
                    {
                        size = hint.Value.Size;
                        break;
                    }
                }
            }

            if (value == null) value = DBNull.Value;

            Parameters.Add(new ParameterInfo(parameterName, value));
            Adapter.SetParameter(parameterName, new SqlServerFluidAdapter.Hint(type, size));
            if (inject)
            {
                Adapter.SetCondition("[" + parameter + "] " + comparison + " " + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public SqlServerFluidSelector SetParameter(string parameter, object value, Type type, bool inject = true, string comparison = "=")
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
                Adapter.SetCondition("[" + parameter + "] " + comparison + " " + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public SqlServerFluidSelector SetParameter(string parameter, object value, bool inject = true, string comparison = "=")
        {
            return SetParameter(parameter, value, null, inject, comparison);            
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public SqlServerFluidSelector Fragment(string fragment)
        {
            Adapter.Fragment(fragment);
            return this;
        }

        /// <summary>
        /// Adds the query condition to the select command.
        /// </summary>
        public SqlServerFluidSelector SetCondition(string condition, string connector = "AND")
        {
            Adapter.SetCondition(condition, connector);
            return this;
        }

        /// <summary>
        /// Returns the dynamically generated SqlServerAdapterConfiguration.
        /// </summary>
        public SqlServerAdapterConfiguration Configuration(DataSet dataset, string table = null)
        {
            if (table == null) table = Table;
            return new SqlServerAdapterConfiguration(dataset, table, SqlAdapter, Parameters, SqlAction.Get);
        }
    }
}

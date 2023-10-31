using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using FluidFramework.Data;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate an MySqlAdapterConfiguration.
    /// </summary>
    public class MySqlFluidSelector
    {
        /// <summary>
        /// The underlying fluid adapter.
        /// </summary>
        public MySqlFluidAdapter Adapter { get; set; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public List<ParameterInfo> Parameters { get; private set; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// The dynamically generated MySqlDataAdapter.
        /// </summary>
        public MySqlDataAdapter MySqlAdapter
        {
            get { return Adapter.Adapter; }
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with the underlying fluid adapter.
        /// </summary>
        public static MySqlFluidSelector New(MySqlFluidAdapter adapter)
        {
            return new MySqlFluidSelector(adapter);
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with a generic select query.
        /// </summary>
        public static MySqlFluidSelector New(string table, string query = null)
        {
            if (query == null) query = "SELECT * FROM `" + table + "`";
            return new MySqlFluidSelector(MySqlFluidAdapter.New.CreateSelect(query, table)) { Table = table };
        }

        /// <summary>
        /// Constructor that initializes the underlying fluid adapter.
        /// </summary>
        public MySqlFluidSelector(MySqlFluidAdapter adapter)
        {
            Adapter = adapter;
            Parameters = new List<ParameterInfo>();
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public MySqlFluidSelector SetParameter(string parameter, object value, MySqlDbType type, int size, bool inject = true, string comparison = "=")
        {
            if (String.IsNullOrEmpty(parameter)) throw new Exception("Undefined parameter name.");
            string parameterName = "@" + Regex.Replace(parameter, "[^\\w\\._]", "");

            if (size == -1)
            {
                foreach (KeyValuePair<Type, MySqlFluidAdapter.Hint> hint in Adapter.TypeHinting)
                {
                    if (hint.Value.MySqlDbType == type)
                    {
                        size = hint.Value.Size;
                        break;
                    }
                }
            }

            if (value == null) value = DBNull.Value;

            Parameters.Add(new ParameterInfo(parameterName, value));
            Adapter.SetParameter(parameterName, new MySqlFluidAdapter.Hint(type, size));
            if (inject)
            {
                Adapter.SetCondition("`" + parameter + "` " + comparison + " " + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public MySqlFluidSelector SetParameter(string parameter, object value, Type type, bool inject = true, string comparison = "=")
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
                Adapter.SetCondition("`" + parameter + "` " + comparison + " " + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the "@" suffix appended.
        /// </summary>
        public MySqlFluidSelector SetParameter(string parameter, object value, bool inject = true, string comparison = "=")
        {
            return SetParameter(parameter, value, null, inject, comparison);
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public MySqlFluidSelector Fragment(string fragment)
        {
            Adapter.Fragment(fragment);
            return this;
        }

        /// <summary>
        /// Adds the query condition to the select command.
        /// </summary>
        public MySqlFluidSelector SetCondition(string condition, string connector = "AND")
        {
            Adapter.SetCondition(condition, connector);
            return this;
        }

        /// <summary>
        /// Returns the dynamically generated MySqlAdapterConfiguration.
        /// </summary>
        public MySqlAdapterConfiguration Configuration(DataSet dataset, string table = null)
        {
            if (table == null) table = Table;
            return new MySqlAdapterConfiguration(dataset, table, MySqlAdapter, Parameters, SqlAction.Get);
        }
    }
}

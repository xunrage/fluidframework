using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using FluidFramework.Data;
using Oracle.ManagedDataAccess.Client;

namespace FluidFramework.Oracle.Data
{
    /// <summary>
    /// Provides a mechanism to dynamically generate an OracleAdapterConfiguration.
    /// </summary>
    public class OracleFluidSelector
    {
        /// <summary>
        /// The underlying fluid adapter.
        /// </summary>
        public OracleFluidAdapter Adapter { get; set; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public List<ParameterInfo> Parameters { get; private set; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// The dynamically generated OracleDataAdapter.
        /// </summary>
        public OracleDataAdapter OracleAdapter
        {
            get { return Adapter.Adapter; }
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with the underlying fluid adapter.
        /// </summary>
        public static OracleFluidSelector New(OracleFluidAdapter adapter)
        {
            return new OracleFluidSelector(adapter);
        }

        /// <summary>
        /// Helper method to create a new FluidSelector instance with a generic select query.
        /// </summary>
        public static OracleFluidSelector New(string table, string query = null)
        {
            if (query == null) query = "SELECT * FROM \"" + table + "\"";
            return new OracleFluidSelector(OracleFluidAdapter.New.CreateSelect(query)) { Table = table };
        }

        /// <summary>
        /// Constructor that initializes the underlying fluid adapter.
        /// </summary>
        public OracleFluidSelector(OracleFluidAdapter adapter)
        {
            Adapter = adapter;
            Parameters = new List<ParameterInfo>();
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the ":p_" suffix appended.
        /// </summary>
        public OracleFluidSelector SetParameter(string parameter, object value, OracleDbType type, int? size = null, bool inject = true, string comparison = "=")
        {
            if (String.IsNullOrEmpty(parameter)) throw new Exception("Undefined parameter name.");
            string parameterName = ":p_" + Regex.Replace(parameter, "[^\\w\\._]", "");

            if (value == null) value = DBNull.Value;

            Parameters.Add(new ParameterInfo(parameterName, value));
            Adapter.Adapter.SelectCommand.Parameters.Add(size.HasValue ? new OracleParameter(parameterName, type, size.Value) : new OracleParameter(parameterName, type));
            if (inject)
            {
                Adapter.SetCondition("\"" + parameter + "\"" + comparison + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the ":p_" suffix appended.
        /// </summary>
        public OracleFluidSelector SetParameter(string parameter, object value, Type type, bool inject = true, string comparison = "=")
        {
            if (String.IsNullOrEmpty(parameter)) throw new Exception("Undefined parameter name.");
            if (value == null && type == null) throw new Exception("Undefined parameter type.");

            string parameterName = ":p_" + Regex.Replace(parameter, "[^\\w\\._]", "");
            Type parameterType = type ?? value.GetType();

            if (value == null) value = DBNull.Value;

            Parameters.Add(new ParameterInfo(parameterName, value));
            Adapter.SetParameter(parameterName, parameterType);
            if (inject)
            {
                Adapter.SetCondition("\"" + parameter + "\"" + comparison + parameterName);
            }
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter list and the select command of the adapter and optionally adds the condition to the query.
        /// The parameter has the ":p_" suffix appended.
        /// </summary>
        public OracleFluidSelector SetParameter(string parameter, object value, bool inject = true, string comparison = "=")
        {
            return SetParameter(parameter, value, null, inject, comparison);
        }

        /// <summary>
        /// Adds the query fragment to the select command.
        /// </summary>
        public OracleFluidSelector Fragment(string fragment)
        {
            Adapter.Fragment(fragment);
            return this;
        }

        /// <summary>
        /// Adds the query condition to the select command.
        /// </summary>
        public OracleFluidSelector SetCondition(string condition, string connector = "AND")
        {
            Adapter.SetCondition(condition, connector);
            return this;
        }

        /// <summary>
        /// Returns the dynamically generated SqlServerAdapterConfiguration.
        /// </summary>
        public OracleAdapterConfiguration Configuration(DataSet dataset, string table = null)
        {
            if (table == null) table = Table;
            return new OracleAdapterConfiguration(dataset, table, OracleAdapter, Parameters, SqlAction.Get);
        }
    }
}

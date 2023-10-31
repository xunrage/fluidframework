using System;
using System.Collections.Generic;
using System.Data;
using FluidFramework.Data;
using Oracle.ManagedDataAccess.Client;

namespace FluidFramework.Oracle.Data
{
    /// <summary>
    /// A configuration used to execute actions over the database.
    /// </summary>
    public class OracleAdapterConfiguration : AdapterConfiguration
    {
        /// <summary>
        /// The OracleDataAdapter object that executes the actions.
        /// </summary>
        public OracleDataAdapter Adapter { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OracleAdapterConfiguration() { }

        /// <summary>
        /// Main constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(DataSet pDataset, String pTableName, OracleDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : base(pDataset, pTableName, pParameterList, pAction, pPriority)
        {
            Adapter = pAdapter;
        }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(DataTable pTable, OracleDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(DataSet pDataset, String pTableName, OracleDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, new List<ParameterInfo> {pParameter}, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(DataTable pTable, OracleDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(DataSet pDataset, String pTableName, OracleDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(DataTable pTable, OracleDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(OracleDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public OracleAdapterConfiguration(OracleDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }
    }
}

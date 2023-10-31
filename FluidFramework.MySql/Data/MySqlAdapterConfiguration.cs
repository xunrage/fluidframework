using System;
using System.Collections.Generic;
using System.Data;
using FluidFramework.Data;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Data
{
    /// <summary>
    /// A configuration used to execute actions over the database.
    /// </summary>
    public class MySqlAdapterConfiguration : AdapterConfiguration
    {
        /// <summary>
        /// The MySqlDataAdapter object that executes the actions.
        /// </summary>
        public MySqlDataAdapter Adapter { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MySqlAdapterConfiguration() { }

        /// <summary>
        /// Main constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(DataSet pDataset, String pTableName, MySqlDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : base(pDataset, pTableName, pParameterList, pAction, pPriority)
        {
            Adapter = pAdapter;
        }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(DataTable pTable, MySqlDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(DataSet pDataset, String pTableName, MySqlDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, new List<ParameterInfo> {pParameter}, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(DataTable pTable, MySqlDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(DataSet pDataset, String pTableName, MySqlDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(DataTable pTable, MySqlDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(MySqlDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public MySqlAdapterConfiguration(MySqlDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }
    }
}

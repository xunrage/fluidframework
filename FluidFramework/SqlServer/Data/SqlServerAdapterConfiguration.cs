using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FluidFramework.Data;

namespace FluidFramework.SqlServer.Data
{
    /// <summary>
    /// A configuration used to execute actions over the database.
    /// </summary>
    public class SqlServerAdapterConfiguration : AdapterConfiguration
    {
        /// <summary>
        /// The SqlDataAdapter object that executes the actions.
        /// </summary>
        public SqlDataAdapter Adapter { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlServerAdapterConfiguration(){}

        /// <summary>
        /// Main constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(DataSet pDataset, String pTableName, SqlDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : base(pDataset, pTableName, pParameterList, pAction, pPriority)
        {
            Adapter = pAdapter;
        }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(DataTable pTable, SqlDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(DataSet pDataset, String pTableName, SqlDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, new List<ParameterInfo> {pParameter}, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(DataTable pTable, SqlDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(DataSet pDataset, String pTableName, SqlDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(DataTable pTable, SqlDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(SqlDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqlServerAdapterConfiguration(SqlDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }
    }
}

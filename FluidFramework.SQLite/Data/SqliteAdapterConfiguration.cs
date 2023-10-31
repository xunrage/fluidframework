using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using FluidFramework.Data;

namespace FluidFramework.SQLite.Data
{
    /// <summary>
    /// A configuration used to execute actions over the database.
    /// </summary>
    public class SqliteAdapterConfiguration : AdapterConfiguration
    {
        /// <summary>
        /// The SQLiteDataAdapter object that executes the actions.
        /// </summary>
        public SQLiteDataAdapter Adapter { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqliteAdapterConfiguration() { }

        /// <summary>
        /// Main constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(DataSet pDataset, String pTableName, SQLiteDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : base(pDataset, pTableName, pParameterList, pAction, pPriority)
        {
            Adapter = pAdapter;
        }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(DataTable pTable, SQLiteDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(DataSet pDataset, String pTableName, SQLiteDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, new List<ParameterInfo> {pParameter}, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(DataTable pTable, SQLiteDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(DataSet pDataset, String pTableName, SQLiteDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pDataset, pTableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(DataTable pTable, SQLiteDataAdapter pAdapter, SqlAction pAction, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(pTable.DataSet, pTable.TableName, pAdapter, (List<ParameterInfo>)null, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(SQLiteDataAdapter pAdapter, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, pParameterList, pAction, pPriority) { }

        /// <summary>
        /// Constructor that allows the initialization of the fields.
        /// </summary>
        public SqliteAdapterConfiguration(SQLiteDataAdapter pAdapter, ParameterInfo pParameter, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
            : this(null, null, pAdapter, new List<ParameterInfo> { pParameter }, pAction, pPriority) { }
    }
}

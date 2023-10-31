using System;
using System.Collections.Generic;
using System.Data;

namespace FluidFramework.Data
{
    /// <summary>
    /// Base configuration for execution of actions over the database.
    /// </summary>
    public abstract class AdapterConfiguration : IAdapterConfiguration
    {
        /// <summary>
        /// The dataset that has the target table.
        /// </summary>
        public DataSet Dataset { get; set; }

        /// <summary>
        /// The table that is used to perform the actions.
        /// </summary>
        public String TableName { get; set; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public List<ParameterInfo> ParameterList { get; set; }

        /// <summary>
        /// The action to execute.
        /// </summary>
        public SqlAction Action { get; set; }

        /// <summary>
        /// The moment when the action is executed.
        /// </summary>
        public SqlPriority Priority { get; set; }

        /// <summary>
        /// Custom command.
        /// </summary>
        public CommandDelegate Command { get; set; }

        /// <summary>
        /// The number of expected rows returned by the query.
        /// </summary>
        public int? ExpectedRows { get; set; }

        private void SetDefaults()
        {
            Action = SqlAction.None;
            Priority = SqlPriority.OnUpdate;
            Command = null;
            ExpectedRows = null;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AdapterConfiguration()
        {
            SetDefaults();
            ParameterList = new List<ParameterInfo>();                        
        }

        /// <summary>
        /// Main constructor that allows the initialization of the fields.
        /// </summary>
        public AdapterConfiguration(DataSet pDataset, String pTableName, List<ParameterInfo> pParameterList = null, SqlAction pAction = SqlAction.None, SqlPriority pPriority = SqlPriority.OnUpdate)
        {
            SetDefaults();
            Dataset = pDataset;
            TableName = pTableName;
            ParameterList = pParameterList ?? new List<ParameterInfo>();
            Action = pAction;
            Priority = pPriority;
        }
    }
}

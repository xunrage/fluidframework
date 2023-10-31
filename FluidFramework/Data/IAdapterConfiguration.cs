using System;
using System.Collections.Generic;
using System.Data;

namespace FluidFramework.Data
{
    /// <summary>
    /// Simple interface for an adapter configuration
    /// </summary>
    public interface IAdapterConfiguration
    {
        /// <summary>
        /// The dataset that has the target table.
        /// </summary>
        DataSet Dataset { get; set; }

        /// <summary>
        /// The table that is used to perform the actions.
        /// </summary>
        String TableName { get; set; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        List<ParameterInfo> ParameterList { get; set; }

        /// <summary>
        /// The action to execute.
        /// </summary>
        SqlAction Action { get; set; }

        /// <summary>
        /// The moment when the action is executed.
        /// </summary>
        SqlPriority Priority { get; set; }

        /// <summary>
        /// Custom command.
        /// </summary>
        CommandDelegate Command { get; set; }

        /// <summary>
        /// The number of expected rows returned by the query.
        /// </summary>
        int? ExpectedRows { get; set; }
    }
}

using System.Collections.Generic;
using FluidFramework.Business;
using FluidFramework.Data;
using FluidFramework.SqlServer.Data;

namespace FluidFramework.SqlServer.Business
{
    /// <summary>
    /// Aggregates business service actions.
    /// </summary>
    public class SqlServerServiceManager : ServiceManager
    {
        /// <summary>
        /// Performs the configuration in database.
        /// </summary>
        protected override void Execute(List<IAdapterConfiguration> configuration)
        {
            List<SqlServerAdapterConfiguration> sqlServerList = new List<SqlServerAdapterConfiguration>();

            foreach (IAdapterConfiguration ac in configuration)
            {
                if (ac is SqlServerAdapterConfiguration) sqlServerList.Add(ac as SqlServerAdapterConfiguration);
            }

            if (sqlServerList.Count > 0)
            {
                new SqlServerDataService().Perform(sqlServerList);
            }
        }
    }
}

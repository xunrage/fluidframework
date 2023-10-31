using System.Collections.Generic;
using FluidFramework.Business;
using FluidFramework.Data;
using FluidFramework.SQLite.Data;

namespace FluidFramework.SQLite.Business
{
    /// <summary>
    /// Aggregates business service actions.
    /// </summary>
    public class SqliteServiceManager : ServiceManager
    {
        /// <summary>
        /// Performs the configuration in database.
        /// </summary>
        protected override void Execute(List<IAdapterConfiguration> configuration)
        {
            List<SqliteAdapterConfiguration> sqliteList = new List<SqliteAdapterConfiguration>();

            foreach (IAdapterConfiguration ac in configuration)
            {
                if (ac is SqliteAdapterConfiguration) sqliteList.Add(ac as SqliteAdapterConfiguration);
            }

            if (sqliteList.Count > 0)
            {
                new SqliteDataService().Perform(sqliteList);
            }
        }
    }
}

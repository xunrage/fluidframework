using System.Collections.Generic;
using FluidFramework.Business;
using FluidFramework.Data;
using FluidFramework.MySql.Data;

namespace FluidFramework.MySql.Business
{
    /// <summary>
    /// Aggregates business service actions.
    /// </summary>
    public class MySqlServiceManager : ServiceManager
    {
        /// <summary>
        /// Performs the configuration in database.
        /// </summary>
        protected override void Execute(List<IAdapterConfiguration> configuration)
        {
            List<MySqlAdapterConfiguration> mySqlList = new List<MySqlAdapterConfiguration>();

            foreach (IAdapterConfiguration ac in configuration)
            {
                if (ac is MySqlAdapterConfiguration) mySqlList.Add(ac as MySqlAdapterConfiguration);
            }

            if (mySqlList.Count > 0)
            {
                new MySqlDataService().Perform(mySqlList);
            }
        }
    }
}

using System.Collections.Generic;
using FluidFramework.Business;
using FluidFramework.Data;
using FluidFramework.Oracle.Data;

namespace FluidFramework.Oracle.Business
{
    /// <summary>
    /// Aggregates business service actions.
    /// </summary>
    public class OracleServiceManager : ServiceManager
    {
        /// <summary>
        /// Performs the configuration in database.
        /// </summary>
        protected override void Execute(List<IAdapterConfiguration> configuration)
        {
            List<OracleAdapterConfiguration> oracleList = new List<OracleAdapterConfiguration>();

            foreach (IAdapterConfiguration ac in configuration)
            {
                if (ac is OracleAdapterConfiguration) oracleList.Add(ac as OracleAdapterConfiguration);
            }

            if (oracleList.Count > 0)
            {
                new OracleDataService().Perform(oracleList);
            }
        }
    }
}

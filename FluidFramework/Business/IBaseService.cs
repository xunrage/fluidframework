using System.Collections.Generic;
using FluidFramework.Data;

namespace FluidFramework.Business
{
    /// <summary>
    /// Interface for a business service
    /// </summary>
    public interface IBaseService
    {
        /// <summary>
        /// Executed before the load operation.
        /// </summary>
        void BeforeLoad();

        /// <summary>
        /// Gets the load configuration.
        /// </summary>
        List<IAdapterConfiguration> GetLoadConfiguration();

        /// <summary>
        /// Executed after the load operation.
        /// </summary>
        void AfterLoad();

        /// <summary>
        /// Executed before the save operation.
        /// </summary>
        void BeforeSave();

        /// <summary>
        /// Gets the save configuration.
        /// </summary>
        List<IAdapterConfiguration> GetSaveConfiguration();

        /// <summary>
        /// Executed after the save operation.
        /// </summary>
        void AfterSave();
    }
}

using System.Collections.Generic;
using System.Data;
using FluidFramework.Data;

namespace FluidFramework.Business
{
    /// <summary>
    /// Inherited by a business service
    /// </summary>
    public class BaseService<TModel> : IBaseService where TModel : DataSet, new()
    {
        /// <summary>
        /// The model that the service is using.
        /// </summary>
        public TModel Model { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseService(TModel model = null)
        {
            Model = model ?? new TModel();
        }        

        /// <summary>
        /// Gets the load configuration.
        /// </summary>
        public virtual List<IAdapterConfiguration> GetLoadConfiguration()
        {
            return new List<IAdapterConfiguration>();
        }

        /// <summary>
        /// Executed before the load operation.
        /// </summary>
        public virtual void BeforeLoad() { }

        /// <summary>
        /// The load operation.
        /// </summary>
        public virtual void ExecuteLoad() { }

        /// <summary>
        /// Executed after the load operation.
        /// </summary>
        public virtual void AfterLoad() { }

        /// <summary>
        /// Gets the save configuration.
        /// </summary>
        public virtual List<IAdapterConfiguration> GetSaveConfiguration()
        {
            return new List<IAdapterConfiguration>();
        }

        /// <summary>
        /// Executed before the save operation.
        /// </summary>
        public virtual void BeforeSave() { }

        /// <summary>
        /// The save operation.
        /// </summary>
        public virtual void ExecuteSave() { }

        /// <summary>
        /// Executed after the save operation.
        /// </summary>
        public virtual void AfterSave(){}
    }
}

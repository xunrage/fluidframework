using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace FluidFramework.Business
{
    /// <summary>
    /// Inherited by a business service that uses a data service
    /// </summary>
    public class ExtendedService<TDataset, TService> : BaseService<TDataset> where TDataset : DataSet, new() where TService : Component, new()
    {
        /// <summary>
        /// The data service that is used.
        /// </summary>
        public TService Service { get; set; }

        /// <summary>
        /// The load command
        /// </summary>
        public string LoadCommand { get; set; }

        /// <summary>
        /// The save command
        /// </summary>
        public string SaveCommand { get; set; }

        /// <summary>
        /// The parameters for the load operation.
        /// </summary>
        public Dictionary<string, object> LoadParameters { get; set; }

        /// <summary>
        /// The parameters for the save operation.
        /// </summary>
        public Dictionary<string, object> SaveParameters { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExtendedService(TDataset model = null, TService service = null) : base(model)
        {
            Service = service ?? new TService();
            ResetCommands();
        }

        /// <summary>
        /// Clears the commands and the associated parameters.
        /// </summary>
        public void ResetCommands()
        {
            LoadCommand = "";
            SaveCommand = "";
            LoadParameters = new Dictionary<string, object>();
            SaveParameters = new Dictionary<string, object>();
        }

    }
}

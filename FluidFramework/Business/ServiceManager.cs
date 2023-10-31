using System.Collections.Generic;
using System.Linq;
using FluidFramework.Data;

namespace FluidFramework.Business
{
    /// <summary>
    /// Aggregates business service actions.
    /// </summary>
    public abstract class ServiceManager
    {
        /// <summary>
        /// Service list managed by the class.
        /// </summary>
        protected readonly List<ServiceWrapper> serviceList = new List<ServiceWrapper>();

        /// <summary>
        /// Registers a service with the service manager.
        /// </summary>
        public void RegisterService(IBaseService service, int priority)
        {
            if (!serviceList.Exists(item => item.Service == service))
            {
                serviceList.Add(new ServiceWrapper {Service = service, Priority = priority});
            }
        }

        /// <summary>
        /// Unregisters a service from the service manager.
        /// </summary>
        public void UnregisterService(IBaseService service)
        {
            if (serviceList.Exists(item => item.Service == service))
            {
                serviceList.Remove(serviceList.First(item => item.Service == service));
            }
        }

        /// <summary>
        /// Performs the configuration in database.
        /// </summary>
        protected virtual void Execute(List<IAdapterConfiguration> configuration)
        {
        }

        /// <summary>
        /// Executes the load operation over all the registered services.
        /// </summary>
        public void Load()
        {
            foreach (IBaseService service in serviceList.OrderBy(item => item.Priority).Select(item => item.Service))
            {
                service.BeforeLoad();
            }

            List<IAdapterConfiguration> configuration = new List<IAdapterConfiguration>();
            foreach (IBaseService service in serviceList.OrderBy(item => item.Priority).Select(item => item.Service))
            {
                configuration.AddRange(service.GetLoadConfiguration());
            }

            Execute(configuration);

            foreach (IBaseService service in serviceList.OrderBy(item => item.Priority).Select(item => item.Service))
            {
                service.AfterLoad();
            }
        }

        /// <summary>
        /// Executes the save operation over all the registered services.
        /// </summary>
        public void Save()
        {            
            foreach (IBaseService service in serviceList.OrderBy(item => item.Priority).Select(item => item.Service))
            {
                service.BeforeSave();
            }

            List<IAdapterConfiguration> configuration = new List<IAdapterConfiguration>();
            foreach (IBaseService service in serviceList.OrderBy(item=>item.Priority).Select(item=>item.Service))
            {
                configuration.AddRange(service.GetSaveConfiguration());
            }

            Execute(configuration);

            foreach (IBaseService service in serviceList.OrderBy(item => item.Priority).Select(item => item.Service))
            {
                service.AfterSave();
            }
        }
    }
}
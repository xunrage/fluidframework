namespace FluidFramework.Business
{
    /// <summary>
    /// Wraps a service and a priority together.
    /// </summary>
    public class ServiceWrapper
    {
        /// <summary>
        /// Business service
        /// </summary>
        public IBaseService Service { get; set; }

        /// <summary>
        /// Service priority
        /// </summary>
        public int Priority { get; set; }
    }
}

namespace FluidFramework.Context
{
    /// <summary>
    /// Base properties class.
    /// </summary>
    public class Properties
    {
        /// <summary>
        /// The default connection that is used when a data service is run.
        /// </summary>
        public Connection CurrentConnection { get; set; }

        /// <summary>
        /// The active user.
        /// </summary>
        public User CurrentUser { get; set; }

        /// <summary>
        /// The order of the persistence operations that are performed by a data service.
        /// </summary>
        public PerformOrder CurrentPerformOrder { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Properties()
        {
            CurrentConnection = null;
            CurrentUser = null;
            CurrentPerformOrder = PerformOrder.UpdateDelete;
        }
    }
}

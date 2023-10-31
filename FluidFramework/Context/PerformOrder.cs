namespace FluidFramework.Context
{
    /// <summary>
    /// The order of the persistence operations that are performed by a data service.
    /// </summary>
    public enum PerformOrder
    {
        /// <summary>
        /// Executes the update operations then the delete operations.
        /// </summary>
        UpdateDelete,

        /// <summary>
        /// Executes the delete operations then the update operations.
        /// </summary>
        DeleteUpdate        
    }
}

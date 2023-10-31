namespace FluidFramework.Context
{
    /// <summary>
    /// Custom storage manager interface for client context.
    /// </summary>
    public interface IContextStorageManager
    {
        /// <summary>
        /// Stores the instance of the client context.
        /// </summary>
        void SetInstance(ClientContext context);

        /// <summary>
        /// Retrieves the instance of the client context.
        /// </summary>
        ClientContext GetInstance();
    }
}

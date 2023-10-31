namespace FluidFramework.Data
{
    /// <summary>
    /// The moment when the action is executed.
    /// </summary>
    public enum SqlPriority
    {
        /// <summary>
        /// The action is executed on the update step
        /// </summary>
        OnUpdate,

        /// <summary>
        /// The action is executed on the delete step
        /// </summary>
        OnDelete
    }
}

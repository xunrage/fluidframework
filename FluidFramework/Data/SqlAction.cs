namespace FluidFramework.Data
{
    /// <summary>
    /// The action to execute.
    /// </summary>
    public enum SqlAction
    {
        /// <summary>
        /// Unspecified action
        /// </summary>
        None,

        /// <summary>
        /// Uses the SELECT commands to bring data
        /// </summary>
        Get,

        /// <summary>
        /// Uses the SELECT commands to update the database.
        /// </summary>
        Execute,

        /// <summary>
        /// Uses INSERT, UPDATE and DELETE commands to update the database from the user dataset.
        /// </summary>
        Update,

        /// <summary>
        /// Runs the custom action.
        /// </summary>
        Run
    }
}

using System;

namespace FluidFramework.Context
{
    /// <summary>
    /// Base connection class.
    /// </summary>
    public class Connection : ICloneable
    {
        /// <summary>
        /// The character array that contains the information about the connection to a database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The wait time before terminating the attempt to execute a command.
        /// </summary>
        public int? CommandTimeout { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Connection(){}

        /// <summary>
        /// A constructor that takes the verbatim connection string.
        /// </summary>
        public Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// A constructor that takes the verbatim connection string and the SQL command timeout.
        /// </summary>
        public Connection(string connectionString, int commandTimeout) : this(connectionString)
        {
            CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Creates a shallow copy of the current object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Tests if the connection can be opened.
        /// </summary>
        public virtual bool TestConnection(int preferredConnectionTimeout = 15)
        {
            return true;
        }
    }
}

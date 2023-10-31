using System.Data;
using FluidFramework.Context;
using Oracle.ManagedDataAccess.Client;

namespace FluidFramework.Oracle.Context
{
    /// <summary>
    /// Stores information for an Oracle server connection.
    /// </summary>
    public class OracleServerConnection : Connection
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OracleServerConnection() { }

        /// <summary>
        /// A constructor that takes the verbatim connection string.
        /// </summary>
        public OracleServerConnection(string connectionString) : base(connectionString) { }

        /// <summary>
        /// A constructor that takes the verbatim connection string and the SQL command timeout.
        /// </summary>
        public OracleServerConnection(string connectionString, int commandTimeout) : base(connectionString, commandTimeout) { }

        /// <summary>
        /// A constructor that takes the connection parameters.
        /// </summary>
        public OracleServerConnection(string databaseHostName, string databaseServiceName, string username, string password, int connectionTimeout = 15, int? port = 1521)
        {
            try
            {
                OracleConnectionStringBuilder csb = new OracleConnectionStringBuilder
                {
                    DataSource = databaseHostName + (port.HasValue ? ":" + port : "") + "/" + databaseServiceName,
                    PersistSecurityInfo = true,
                    UserID = username,
                    Password = password,
                    ConnectionTimeout = connectionTimeout
                };
                ConnectionString = csb.ToString();
            }
            catch
            {
                ConnectionString = null;
            }
        }

        /// /// <summary>
        /// A constructor that takes the connection parameters and the command timeout.
        /// </summary>
        public OracleServerConnection(string databaseHostName, string databaseServiceName, string username, string password, int connectionTimeout, int? port, int commandTimeout) :
            this(databaseHostName, databaseServiceName, username, password, connectionTimeout, port)
        {
            CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Tests if the connection can be opened.
        /// </summary>
        public override bool TestConnection(int preferredConnectionTimeout = 15)
        {
            OracleConnection connection = null;
            try
            {
                OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder(ConnectionString)
                {
                    ConnectionTimeout = preferredConnectionTimeout
                };
                connection = new OracleConnection(builder.ToString());
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}

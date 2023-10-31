using System;
using System.Data;
using FluidFramework.Context;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Context
{
    /// <summary>
    /// Stores information for a MySQL server connection.
    /// </summary>
    public class MySqlServerConnection : Connection
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MySqlServerConnection() { }

        /// <summary>
        /// A constructor that takes the verbatim connection string.
        /// </summary>
        public MySqlServerConnection(string connectionString) : base(connectionString) { }

        /// <summary>
        /// A constructor that takes the verbatim connection string and the SQL command timeout.
        /// </summary>
        public MySqlServerConnection(string connectionString, int commandTimeout) : base(connectionString, commandTimeout) { }

        /// <summary>
        /// A constructor that takes the connection parameters.
        /// </summary>
        public MySqlServerConnection(string server, string database, string username, string password, int connectionTimeout = 15, int? port = 3306)
        {
            try
            {
                MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder
                {
                    Server = server,
                    Database = database,
                    PersistSecurityInfo = true,
                    UserID = username,
                    Password = password,
                    ConnectionTimeout = Convert.ToUInt32(connectionTimeout)                    
                };
                if (port.HasValue)
                {
                    csb.Port = Convert.ToUInt32(port.Value);
                }
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
        public MySqlServerConnection(string server, string database, string username, string password, int connectionTimeout, int? port, int commandTimeout) :
            this(server, database, username, password, connectionTimeout, port)
        {
            CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Tests if the connection can be opened.
        /// </summary>
        public override bool TestConnection(int preferredConnectionTimeout = 15)
        {
            MySqlConnection connection = null;
            try
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(ConnectionString)
                {
                    ConnectionTimeout = Convert.ToUInt32(preferredConnectionTimeout)
                };
                connection = new MySqlConnection(builder.ToString());
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

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using FluidFramework.Context;

namespace FluidFramework.SqlServer.Context
{
    /// <summary>
    /// Stores information for a SQL Server connection.
    /// </summary>
    public class SqlServerConnection : Connection
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlServerConnection() { }

        /// <summary>
        /// A constructor that takes the verbatim connection string.
        /// </summary>
        public SqlServerConnection(string connectionString) : base(connectionString) { }

        /// <summary>
        /// A constructor that takes the verbatim connection string and the SQL command timeout.
        /// </summary>
        public SqlServerConnection(string connectionString, int commandTimeout) : base(connectionString, commandTimeout) { }

        /// <summary>
        /// A constructor that takes the connection parameters.
        /// </summary>
        public SqlServerConnection(string serverName, string databaseName, bool isIntegrated, string username = null, string password = null, int connectionTimeout = 15,
                                   bool compatibilityMode = false)
        {
            try
            {
                if (compatibilityMode)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Server=");
                    sb.Append(EscapeValue(serverName));
                    sb.Append(";");
                    if (!String.IsNullOrEmpty(databaseName))
                    {
                        sb.Append("Database=");
                        sb.Append(EscapeValue(databaseName));
                        sb.Append(";");
                    }

                    if (!isIntegrated)
                    {
                        sb.Append("User Id=");
                        sb.Append(EscapeValue(username));
                        sb.Append(";");
                        sb.Append("Password=");
                        sb.Append(EscapeValue(password));
                        sb.Append(";");
                    }
                    else
                    {
                        sb.Append("Trusted_Connection=True;");
                    }
                    sb.Append("Connect Timeout=");
                    sb.Append(connectionTimeout.ToString());
                    sb.Append(";");
                    ConnectionString = sb.ToString();
                }
                else
                {
                    SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder
                    {
                        DataSource = serverName,
                        InitialCatalog = databaseName,
                        IntegratedSecurity = isIntegrated,
                        ConnectTimeout = connectionTimeout
                    };
                    if (!isIntegrated)
                    {
                        csb.UserID = username;
                        csb.Password = password;
                    }
                    ConnectionString = csb.ToString();
                }
            }
            catch
            {
                ConnectionString = null;
            }
        }

        /// /// <summary>
        /// A constructor that takes the connection parameters and the SQL command timeout.
        /// </summary>
        public SqlServerConnection(string serverName, string databaseName, bool isIntegrated, string username, string password, int connectionTimeout,
                                   int commandTimeout, bool compatibilityMode = false) :
               this(serverName, databaseName, isIntegrated, username, password, connectionTimeout, compatibilityMode)
        {
            CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Tests if the connection can be opened.
        /// </summary>
        public override bool TestConnection(int preferredConnectionTimeout = 15)
        {
            SqlConnection connection = null;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString)
                {
                    ConnectTimeout = preferredConnectionTimeout
                };
                connection = new SqlConnection(builder.ToString());
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

        /// <summary>
        /// Escapes the value to make a valid connection string.
        /// </summary>
        private string EscapeValue(string value)
        {
            if (value == null) return "";
            Regex quoteRegex = new Regex("^[^\"'=;\\s\\p{Cc}]*$", RegexOptions.Compiled);
            if (quoteRegex.IsMatch(value)) return value;
            if (value.IndexOf('"') != -1 && value.IndexOf('\'') == -1)
            {
                return "'" + value + "'";
            }
            return '"' + value.Replace("\"", "\"\"") + '"';            
        }
    }
}

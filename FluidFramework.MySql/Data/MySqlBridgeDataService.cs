using System.Collections.Generic;
using System.ComponentModel;
using FluidFramework.Data;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Data
{
    /// <summary>
    /// A class that exposes the protected methods of a data service.
    /// </summary>
    public sealed partial class MySqlBridgeDataService : MySqlDataService
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MySqlBridgeDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public MySqlBridgeDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public MySqlBridgeDataService(MySqlConnection connection, MySqlTransaction transaction)
        {
            InitializeComponent();
            GlobalInitialize(connection, transaction);
        }

        /// <summary>
        /// Constructor that allows the service to inherit the configuration.
        /// </summary>
        public MySqlBridgeDataService(MySqlDataService service) : this()
        {
            if (service != null)
            {
                _connectionString = service.ConnectionString;
                _commandTimeout = service.CommandTimeout;
                _autoRefreshConnection = service.AutoRefreshConnection;
                _globalConnection = service.GlobalConnection;
                _globalTransaction = service.GlobalTransaction;
                _useGlobalConnectivity = service.UseGlobalConnectivity;
                _useTransaction = service.UseTransaction;
                _performOrder = service.PerformOrder;
            }
        }

        /// <summary>
        /// Creates a new MySQL connection with the given connection string or with the connection string from context.
        /// </summary>
        public new MySqlConnection CreateConnection()
        {
            return base.CreateConnection();
        }

        /// <summary>
        /// Opens the given MySQL connection.
        /// </summary>
        public new MySqlConnection OpenConnection(MySqlConnection sqlConnection = null)
        {
            return base.OpenConnection(sqlConnection);
        }

        /// <summary>
        /// Opens a transactional MySQL connection and returns the connection and the transaction.
        /// </summary>
        public new MySqlConnection OpenTransactionalConnection(MySqlConnection sqlConnection, out MySqlTransaction sqlTransaction)
        {
            return base.OpenTransactionalConnection(sqlConnection, out sqlTransaction);
        }

        /// <summary>
        /// Closes the given MySQL connection.
        /// </summary>
        public new void CloseConnection(MySqlConnection sqlConnection)
        {
            base.CloseConnection(sqlConnection);
        }

        /// <summary>
        /// Commits and frees the given MySQL transaction.
        /// </summary>
        public new void CommitTransaction(MySqlTransaction sqlTransaction)
        {
            base.CommitTransaction(sqlTransaction);
        }

        /// <summary>
        /// Rolls back and frees the given MySQL transaction.
        /// </summary>
        public new void RollbackTransaction(MySqlTransaction sqlTransaction)
        {
            base.RollbackTransaction(sqlTransaction);
        }

        /// <summary>
        /// Sets the sql command parameters from the parameter info list.
        /// </summary>
        public new void SetParameters(MySqlCommand sqlCommand, List<ParameterInfo> parameterList)
        {
            base.SetParameters(sqlCommand, parameterList);
        }

        /// <summary>
        /// Sets the connection and the transaction on the sql command.
        /// </summary>
        public new void SetCommand(MySqlCommand sqlCommand, MySqlConnection sqlConnection, MySqlTransaction sqlTransaction = null)
        {
            base.SetCommand(sqlCommand, sqlConnection, sqlTransaction);
        }

        /// <summary>
        /// Clears the connection and the transaction from the sql command.
        /// </summary>
        public new void UnsetCommand(MySqlCommand sqlCommand)
        {
            base.UnsetCommand(sqlCommand);
        }
    }
}

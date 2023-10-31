using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using FluidFramework.Data;

namespace FluidFramework.SqlServer.Data
{
    /// <summary>
    /// A class that exposes the protected methods of a data service.
    /// </summary>
    public sealed partial class SqlServerBridgeDataService : SqlServerDataService
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlServerBridgeDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public SqlServerBridgeDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public SqlServerBridgeDataService(SqlConnection connection, SqlTransaction transaction)
        {
            InitializeComponent();
            GlobalInitialize(connection, transaction);
        }

        /// <summary>
        /// Constructor that allows the service to inherit the configuration.
        /// </summary>
        public SqlServerBridgeDataService(SqlServerDataService service) : this()
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
        /// Creates a new SQL connection with the given connection string or with the connection string from context.
        /// </summary>
        public new SqlConnection CreateConnection()
        {
            return base.CreateConnection();
        }

        /// <summary>
        /// Opens the given SQL connection.
        /// </summary>
        public new SqlConnection OpenConnection(SqlConnection sqlConnection = null)
        {
            return base.OpenConnection(sqlConnection);
        }

        /// <summary>
        /// Opens a transactional SQL connection and returns the connection and the transaction.
        /// </summary>
        public new SqlConnection OpenTransactionalConnection(SqlConnection sqlConnection, out SqlTransaction sqlTransaction)
        {
            return base.OpenTransactionalConnection(sqlConnection, out sqlTransaction);
        }

        /// <summary>
        /// Closes the given SQL connection.
        /// </summary>
        public new void CloseConnection(SqlConnection sqlConnection)
        {
            base.CloseConnection(sqlConnection);
        }

        /// <summary>
        /// Commits and frees the given SQL transaction.
        /// </summary>
        public new void CommitTransaction(SqlTransaction sqlTransaction)
        {
            base.CommitTransaction(sqlTransaction);
        }

        /// <summary>
        /// Rolls back and frees the given SQL transaction.
        /// </summary>
        public new void RollbackTransaction(SqlTransaction sqlTransaction)
        {
            base.RollbackTransaction(sqlTransaction);
        }

        /// <summary>
        /// Sets the sql command parameters from the parameter info list.
        /// </summary>
        public new void SetParameters(SqlCommand sqlCommand, List<ParameterInfo> parameterList)
        {
            base.SetParameters(sqlCommand, parameterList);
        }

        /// <summary>
        /// Sets the connection and the transaction on the sql command.
        /// </summary>
        public new void SetCommand(SqlCommand sqlCommand, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
        {
            base.SetCommand(sqlCommand, sqlConnection, sqlTransaction);
        }

        /// <summary>
        /// Clears the connection and the transaction from the sql command.
        /// </summary>
        public new void UnsetCommand(SqlCommand sqlCommand)
        {
            base.UnsetCommand(sqlCommand);
        }
    }
}

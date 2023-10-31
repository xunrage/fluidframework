using System.Collections.Generic;
using System.ComponentModel;
using FluidFramework.Data;
using Oracle.ManagedDataAccess.Client;

namespace FluidFramework.Oracle.Data
{
    /// <summary>
    /// A class that exposes the protected methods of a data service.
    /// </summary>
    public sealed partial class OracleBridgeDataService : OracleDataService
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public OracleBridgeDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public OracleBridgeDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public OracleBridgeDataService(OracleConnection connection, OracleTransaction transaction)
        {
            InitializeComponent();
            GlobalInitialize(connection, transaction);
        }

        /// <summary>
        /// Constructor that allows the service to inherit the configuration.
        /// </summary>
        public OracleBridgeDataService(OracleDataService service) : this()
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
        /// Creates a new Oracle connection with the given connection string or with the connection string from context.
        /// </summary>
        public new OracleConnection CreateConnection()
        {
            return base.CreateConnection();
        }

        /// <summary>
        /// Opens the given Oracle connection.
        /// </summary>
        public new OracleConnection OpenConnection(OracleConnection sqlConnection = null)
        {
            return base.OpenConnection(sqlConnection);
        }

        /// <summary>
        /// Opens a transactional Oracle connection and returns the connection and the transaction.
        /// </summary>
        public new OracleConnection OpenTransactionalConnection(OracleConnection sqlConnection, out OracleTransaction sqlTransaction)
        {
            return base.OpenTransactionalConnection(sqlConnection, out sqlTransaction);
        }

        /// <summary>
        /// Closes the given Oracle connection.
        /// </summary>
        public new void CloseConnection(OracleConnection sqlConnection)
        {
            base.CloseConnection(sqlConnection);
        }

        /// <summary>
        /// Commits and frees the given Oracle transaction.
        /// </summary>
        public new void CommitTransaction(OracleTransaction sqlTransaction)
        {
            base.CommitTransaction(sqlTransaction);
        }

        /// <summary>
        /// Rolls back and frees the given Oracle transaction.
        /// </summary>
        public new void RollbackTransaction(OracleTransaction sqlTransaction)
        {
            base.RollbackTransaction(sqlTransaction);
        }

        /// <summary>
        /// Sets the sql command parameters from the parameter info list.
        /// </summary>
        public new void SetParameters(OracleCommand sqlCommand, List<ParameterInfo> parameterList)
        {
            base.SetParameters(sqlCommand, parameterList);
        }

        /// <summary>
        /// Sets the connection and the transaction on the sql command.
        /// </summary>
        public new void SetCommand(OracleCommand sqlCommand, OracleConnection sqlConnection, OracleTransaction sqlTransaction = null)
        {
            base.SetCommand(sqlCommand, sqlConnection, sqlTransaction);
        }

        /// <summary>
        /// Clears the connection and the transaction from the sql command.
        /// </summary>
        public new void UnsetCommand(OracleCommand sqlCommand)
        {
            base.UnsetCommand(sqlCommand);
        }
    }
}

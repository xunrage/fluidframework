using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using FluidFramework.Data;

namespace FluidFramework.SQLite.Data
{
    /// <summary>
    /// A class that exposes the protected methods of a data service.
    /// </summary>
    public partial class SqliteBridgeDataService : SqliteDataService
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqliteBridgeDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public SqliteBridgeDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public SqliteBridgeDataService(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            InitializeComponent();
            GlobalInitialize(connection, transaction);
        }

        /// <summary>
        /// Constructor that allows the service to inherit the configuration.
        /// </summary>
        public SqliteBridgeDataService(SqliteDataService service) : this()
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
        /// Creates a new SQLite connection with the given connection string or with the connection string from context.
        /// </summary>
        public new SQLiteConnection CreateConnection()
        {
            return base.CreateConnection();
        }

        /// <summary>
        /// Opens the given SQLite connection.
        /// </summary>
        public new SQLiteConnection OpenConnection(SQLiteConnection sqlConnection = null)
        {
            return base.OpenConnection(sqlConnection);
        }

        /// <summary>
        /// Opens a transactional SQLite connection and returns the connection and the transaction.
        /// </summary>
        public new SQLiteConnection OpenTransactionalConnection(SQLiteConnection sqlConnection, out SQLiteTransaction sqlTransaction)
        {
            return base.OpenTransactionalConnection(sqlConnection, out sqlTransaction);
        }

        /// <summary>
        /// Closes the given SQLite connection.
        /// </summary>
        public new void CloseConnection(SQLiteConnection sqlConnection)
        {
            base.CloseConnection(sqlConnection);
        }

        /// <summary>
        /// Commits and frees the given SQLite transaction.
        /// </summary>
        public new void CommitTransaction(SQLiteTransaction sqlTransaction)
        {
            base.CommitTransaction(sqlTransaction);
        }

        /// <summary>
        /// Rolls back and frees the given SQLite transaction.
        /// </summary>
        public new void RollbackTransaction(SQLiteTransaction sqlTransaction)
        {
            base.RollbackTransaction(sqlTransaction);
        }

        /// <summary>
        /// Sets the sql command parameters from the parameter info list.
        /// </summary>
        public new void SetParameters(SQLiteCommand sqlCommand, List<ParameterInfo> parameterList)
        {
            base.SetParameters(sqlCommand, parameterList);
        }

        /// <summary>
        /// Sets the connection and the transaction on the sql command.
        /// </summary>
        public new void SetCommand(SQLiteCommand sqlCommand, SQLiteConnection sqlConnection, SQLiteTransaction sqlTransaction = null)
        {
            base.SetCommand(sqlCommand, sqlConnection, sqlTransaction);
        }

        /// <summary>
        /// Clears the connection and the transaction from the sql command.
        /// </summary>
        public new void UnsetCommand(SQLiteCommand sqlCommand)
        {
            base.UnsetCommand(sqlCommand);
        }
    }
}
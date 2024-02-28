using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using FluidFramework.Context;
using FluidFramework.Data;

namespace FluidFramework.SQLite.Data
{
    /// <summary>
    /// Inherited by a data service that works with SQLite.
    /// </summary>
    public partial class SqliteDataService : Component
    {
        #region Properties

        /// <summary>
        /// Protected member for connection string used to create the connection to the SQLite.
        /// </summary>
        protected string _connectionString;

        /// <summary>
        /// Protected member for the wait time before terminating the attempt to execute a command.
        /// </summary>
        protected int? _commandTimeout;

        /// <summary>
        /// Protected member for signaling the connection renewal.
        /// </summary>
        protected bool _autoRefreshConnection;

        /// <summary>
        /// Protected member for the global connection object.
        /// </summary>
        protected SQLiteConnection _globalConnection;

        /// <summary>
        /// Protected member for the global transaction object.
        /// </summary>
        protected SQLiteTransaction _globalTransaction;

        /// <summary>
        /// Protected member for signaling the usage of global connection.
        /// </summary>
        protected bool _useGlobalConnectivity;

        /// <summary>
        /// Protected member that specifies if transactions are used.
        /// </summary>
        protected bool? _useTransaction;

        /// <summary>
        /// Protected member for the order of the persistence operations that are performed by the service.
        /// </summary>
        protected PerformOrder _performOrder;

        /// <summary>
        /// Protected member for signaling that the global transaction should be rolled back.
        /// </summary>
        protected bool? _forceRollbackGlobalTransaction;

        /// <summary>
        /// Connection string used to create the connection to the SQLite.        
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                _autoRefreshConnection = false;
            }
        }

        /// <summary>
        /// The wait time before terminating the attempt to execute a command.
        /// </summary>
        public int? CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                _commandTimeout = value;
                _autoRefreshConnection = false;
            }
        }

        /// <summary>
        /// Readonly property that signals the connection renewal.
        /// </summary>
        public bool AutoRefreshConnection
        {
            get { return _autoRefreshConnection; }
        }

        /// <summary>
        /// The global connection object.
        /// </summary>
        public SQLiteConnection GlobalConnection
        {
            get { return _globalConnection; }
            set
            {
                _globalConnection = value;
                _useGlobalConnectivity = _globalConnection != null;
            }
        }

        /// <summary>
        /// The global transaction object.
        /// </summary>
        public SQLiteTransaction GlobalTransaction
        {
            get { return _globalTransaction; }
            set
            {
                _globalTransaction = value;
                _useTransaction = _globalTransaction != null;
            }
        }

        /// <summary>
        /// Readonly property that signals the usage of global connection.
        /// </summary>
        public bool UseGlobalConnectivity
        {
            get { return _useGlobalConnectivity; }
        }

        /// <summary>
        /// Specifies if transactions are used.
        /// </summary>
        public bool? UseTransaction
        {
            get { return _useTransaction; }
            set { _useTransaction = value; }
        }

        /// <summary>
        /// The order of the persistence operations that are performed by the service.
        /// </summary>
        public PerformOrder PerformOrder
        {
            get { return _performOrder; }
            set { _performOrder = value; }
        }

        /// <summary>
        /// Signals that the global transaction should be rolled back.
        /// </summary>
        public bool? ForceRollbackGlobalTransaction
        {
            get { return _forceRollbackGlobalTransaction; }
            set { _forceRollbackGlobalTransaction = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqliteDataService()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public SqliteDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// Helper property to create a new DataService instance.
        /// </summary>
        public static SqliteDataService New
        {
            get
            {
                return new SqliteDataService();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the service.
        /// </summary>
        protected void Initialize()
        {
            _connectionString = null;
            _commandTimeout = null;
            _autoRefreshConnection = true;
            _globalConnection = null;
            _globalTransaction = null;
            _useGlobalConnectivity = false;
            _useTransaction = null;
            _performOrder = ClientContext.Instance.Properties.CurrentPerformOrder;
            _forceRollbackGlobalTransaction = null;
        }

        /// <summary>
        /// Sets the global connection, transaction and command timeout.
        /// </summary>
        public SqliteDataService GlobalInitialize(SQLiteConnection connection, SQLiteTransaction transaction, int? commandTimeout,
                                                  bool? forceRollbackGlobalTransaction = null)
        {
            _connectionString = null;
            _commandTimeout = commandTimeout;
            _autoRefreshConnection = false;
            GlobalConnection = connection;      // sets _useGlobalConnectivity
            GlobalTransaction = transaction;    // sets _useTransaction
            _performOrder = ClientContext.Instance.Properties.CurrentPerformOrder;
            _forceRollbackGlobalTransaction = forceRollbackGlobalTransaction;

            return this;
        }

        /// <summary>
        /// Inherits all the properties from another service.
        /// </summary>
        public SqliteDataService ServiceInitialize(SqliteDataService service)
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
                _forceRollbackGlobalTransaction = service.ForceRollbackGlobalTransaction;
            }

            return this;
        }

        /// <summary>
        /// Checks if the expected number of rows were persisted successfully to the database.
        /// </summary>
        protected void CheckAffectedRows(SqliteAdapterConfiguration ac, int affectedRows)
        {
            if (ac.ExpectedRows.HasValue)
            {
                if (affectedRows != ac.ExpectedRows)
                {
                    throw new Exception("The expected number of rows were not persisted successfully to the database.");
                }
            }
        }

        /// <summary>
        /// Performs the persistence operations.
        /// </summary>
        protected void PerformLoop(List<SqliteAdapterConfiguration> listAdapterConfiguration, bool isUpdate)
        {
            string sqlTableName = null;
            try
            {
                for (int index = 0; index < listAdapterConfiguration.Count; index++)
                {
                    sqlTableName = null;
                    SqliteAdapterConfiguration ac = listAdapterConfiguration[isUpdate ? index : listAdapterConfiguration.Count - 1 - index];
                    if (ac.Action == SqlAction.Update)
                    {
                        sqlTableName = ac.TableName;
                        DataRow[] targetRows = ac.Dataset.Tables[sqlTableName].Select(null, null, isUpdate ? DataViewRowState.Added | DataViewRowState.ModifiedCurrent : DataViewRowState.Deleted);
                        if (targetRows.Length == 0)
                        {
                            continue;
                        }
                        if (ac.Adapter.Update(targetRows) != targetRows.Length)
                        {
                            throw new Exception("Changes were not persisted successfully to the database.");
                        }
                    }
                    else if (ac.Priority == (isUpdate ? SqlPriority.OnUpdate : SqlPriority.OnDelete))
                    {
                        switch (ac.Action)
                        {
                            case SqlAction.Get:
                                CheckAffectedRows(ac, String.IsNullOrEmpty(ac.TableName) ? ac.Adapter.Fill(ac.Dataset) : ac.Adapter.Fill(ac.Dataset, sqlTableName = ac.TableName));
                                break;
                            case SqlAction.Execute:
                                CheckAffectedRows(ac, ac.Adapter.SelectCommand.ExecuteNonQuery());
                                break;
                            case SqlAction.Run:
                                if (ac.Command != null)
                                {
                                    if (!ac.Command())
                                    {
                                        throw new Exception("Custom command failed.");
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception((String.IsNullOrEmpty(sqlTableName) ? "" : "Error occured when table [" + sqlTableName + "] was affected. ") + exception.Message, exception);
            }
        }

        /// <summary>
        /// Uses the configuration to perform the required actions over the database.
        /// </summary>
        public void Perform(List<SqliteAdapterConfiguration> listAdapterConfiguration)
        {
            SQLiteConnection sqlConnection = null;
            SQLiteTransaction sqlTransaction = null;

            try
            {
                if (listAdapterConfiguration == null)
                {
                    throw new Exception("Invalid adapter configuration.");
                }

                if (_useGlobalConnectivity)
                {
                    sqlConnection = _globalConnection;
                    sqlTransaction = _globalTransaction;
                }
                else
                {
                    sqlConnection = _useTransaction.HasValue ?
                        (_useTransaction.Value ? OpenTransactionalConnection(null, out sqlTransaction) : OpenConnection()) :
                        (listAdapterConfiguration.Any(ac => ac.Action == SqlAction.Execute || ac.Action == SqlAction.Update) ?
                            OpenTransactionalConnection(null, out sqlTransaction) : OpenConnection());
                }

                // Prepare
                foreach (SqliteAdapterConfiguration ac in listAdapterConfiguration)
                {
                    if (ac.Action != SqlAction.Run && ac.Adapter == null)
                    {
                        throw new Exception("No adapter was provided.");
                    }

                    switch (ac.Action)
                    {
                        case SqlAction.None: throw new Exception("Action is unknown.");
                        case SqlAction.Get:
                        case SqlAction.Execute:
                            SetParameters(ac.Adapter.SelectCommand, ac.ParameterList);
                            SetCommand(ac.Adapter.SelectCommand, sqlConnection, sqlTransaction);
                            break;
                        case SqlAction.Update:
                            if (ac.Dataset.Tables[ac.TableName] == null)
                            {
                                throw new Exception("Table does not exist in dataset.");
                            }
                            SetCommand(ac.Adapter.InsertCommand, sqlConnection, sqlTransaction);
                            SetCommand(ac.Adapter.UpdateCommand, sqlConnection, sqlTransaction);
                            SetCommand(ac.Adapter.DeleteCommand, sqlConnection, sqlTransaction);
                            break;
                    }
                }

                // Persistence
                if (_performOrder == PerformOrder.UpdateDelete)
                {
                    PerformLoop(listAdapterConfiguration, true);
                    PerformLoop(listAdapterConfiguration, false);
                }
                else
                {
                    PerformLoop(listAdapterConfiguration, false);
                    PerformLoop(listAdapterConfiguration, true);
                }                

                if (!_useGlobalConnectivity && sqlTransaction != null) CommitTransaction(sqlTransaction);

                // Unprepare
                foreach (SqliteAdapterConfiguration ac in listAdapterConfiguration)
                {
                    switch (ac.Action)
                    {
                        case SqlAction.Get:
                        case SqlAction.Execute:
                            UnsetCommand(ac.Adapter.SelectCommand);
                            break;
                        case SqlAction.Update:
                            UnsetCommand(ac.Adapter.InsertCommand);
                            UnsetCommand(ac.Adapter.UpdateCommand);
                            UnsetCommand(ac.Adapter.DeleteCommand);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                if (!_useGlobalConnectivity && sqlTransaction != null) RollbackTransaction(sqlTransaction);
                throw new Exception("Error: Perform method failed. " + exception.Message, exception);
            }
            finally
            {
                if (!_useGlobalConnectivity) CloseConnection(sqlConnection);
            }
        }

        /// <summary>
        /// Uses the configuration to perform the required actions over the database.
        /// </summary>
        public void Perform(SqliteAdapterConfiguration adapterConfiguration)
        {
            Perform(new List<SqliteAdapterConfiguration> { adapterConfiguration });
        }

        /// <summary>
        /// Uses the command to perform multiple operations in a global transaction.
        /// </summary>
        public void MultiplePerform(CommandDelegate command)
        {
            _globalConnection = null;
            _globalTransaction = null;

            try
            {
                _useGlobalConnectivity = true;
                _forceRollbackGlobalTransaction = false;

                _globalConnection = _useTransaction.HasValue ?
                    (_useTransaction.Value ? OpenTransactionalConnection(null, out _globalTransaction) : OpenConnection()) :
                    OpenTransactionalConnection(null, out _globalTransaction);

                if (command != null) if (!command()) throw new Exception("Error: Execution failed.");

                if (_globalTransaction != null)
                {
                    if (_forceRollbackGlobalTransaction.HasValue && _forceRollbackGlobalTransaction.Value)
                    {
                        RollbackTransaction(_globalTransaction);
                    }
                    else
                    {
                        CommitTransaction(_globalTransaction);
                    }
                }
            }
            catch (Exception exception)
            {
                if (_globalTransaction != null)
                {
                    RollbackTransaction(_globalTransaction);
                }

                throw new Exception("MultiplePerform method failed. " + exception.Message, exception);
            }
            finally
            {
                CloseConnection(_globalConnection);
                _forceRollbackGlobalTransaction = null;
                _useGlobalConnectivity = false;
                _globalTransaction = null;
                _globalConnection = null;
            }
        }

        /// <summary>
        /// Creates a new SQLite connection with the given connection string or with the connection string from context.
        /// </summary>
        protected virtual SQLiteConnection CreateConnection()
        {
            if (String.IsNullOrEmpty(_connectionString) || _autoRefreshConnection)
            {
                _connectionString = null;
                _commandTimeout = null;
                Connection connection = ClientContext.Instance.Properties.CurrentConnection;
                if (connection != null)
                {
                    if (!String.IsNullOrEmpty(connection.ConnectionString)) _connectionString = connection.ConnectionString;
                    if (connection.CommandTimeout.HasValue) _commandTimeout = connection.CommandTimeout;
                }
            }
            if (String.IsNullOrEmpty(_connectionString)) throw new Exception("No connection string was provided.");
            return new SQLiteConnection(_connectionString);
        }

        /// <summary>
        /// Opens the given SQLite connection.
        /// </summary>
        protected SQLiteConnection OpenConnection(SQLiteConnection sqlConnection = null)
        {
            if (sqlConnection == null) sqlConnection = CreateConnection();
            try
            {
                sqlConnection.Open();
                return sqlConnection;
            }
            catch (Exception e)
            {
                throw (new Exception("Database connection did not succeed: " + sqlConnection.DataSource + "(" + sqlConnection.Database + "). " + e.Message, e));
            }
        }

        /// <summary>
        /// Opens a transactional SQLite connection and returns the connection and the transaction.
        /// </summary>
        protected SQLiteConnection OpenTransactionalConnection(SQLiteConnection sqlConnection, out SQLiteTransaction sqlTransaction)
        {
            // Connection
            sqlConnection = OpenConnection(sqlConnection);

            // Transaction
            sqlTransaction = sqlConnection.BeginTransaction();
            
            return sqlConnection;
        }

        /// <summary>
        /// Closes the given SQLite connection.
        /// </summary>
        protected void CloseConnection(SQLiteConnection sqlConnection)
        {
            if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
        }

        /// <summary>
        /// Commits and frees the given SQLite transaction.
        /// </summary>
        protected void CommitTransaction(SQLiteTransaction sqlTransaction)
        {
            if (sqlTransaction != null)
            {
                sqlTransaction.Commit();
                sqlTransaction.Dispose();
            }
        }

        /// <summary>
        /// Rolls back and frees the given SQLite transaction.
        /// </summary>
        protected void RollbackTransaction(SQLiteTransaction sqlTransaction)
        {
            if (sqlTransaction != null)
            {
                sqlTransaction.Rollback();
                sqlTransaction.Dispose();
            }
        }

        /// <summary>
        /// Sets the sql command parameters from the parameter info list.
        /// </summary>
        protected void SetParameters(SQLiteCommand sqlCommand, List<ParameterInfo> parameterList)
        {
            if (parameterList != null)
            {
                foreach (ParameterInfo parameterInfo in parameterList)
                {
                    if (sqlCommand.Parameters.Contains(parameterInfo.Parameter))
                    {
                        sqlCommand.Parameters[parameterInfo.Parameter].Value = parameterInfo.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the connection and the transaction on the sql command.
        /// </summary>
        protected void SetCommand(SQLiteCommand sqlCommand, SQLiteConnection sqlConnection, SQLiteTransaction sqlTransaction = null)
        {
            if (sqlCommand != null)
            {
                sqlCommand.Connection = sqlConnection;
                if (_commandTimeout.HasValue) sqlCommand.CommandTimeout = _commandTimeout.Value;
                if (sqlTransaction != null)
                {
                    sqlCommand.Transaction = sqlTransaction;
                }
            }
        }

        /// <summary>
        /// Clears the connection and the transaction from the sql command.
        /// </summary>
        protected void UnsetCommand(SQLiteCommand sqlCommand)
        {
            if (sqlCommand != null)
            {
                sqlCommand.Connection = null;
                sqlCommand.Transaction = null;
            }
        }

        /// <summary>
        /// Opens the global transactional connection.
        /// </summary>
        public void OpenGlobalConnection()
        {
            GlobalConnection = OpenTransactionalConnection(null, out _globalTransaction);
        }

        /// <summary>
        /// Closes the global connection.
        /// </summary>
        public void CloseGlobalConnection()
        {
            CloseConnection(GlobalConnection);
            GlobalConnection = null;
        }

        /// <summary>
        /// Commits the global transaction.
        /// </summary>
        public void CommitGlobalTransaction()
        {
            CommitTransaction(GlobalTransaction);
            GlobalTransaction = null;
        }

        /// <summary>
        /// Rolls back the global transaction.
        /// </summary>
        public void RollbackGlobalTransaction()
        {
            RollbackTransaction(GlobalTransaction);
            GlobalTransaction = null;
        }

        /// <summary>
        /// Inherits global connectivity from another service.
        /// </summary>
        public void ShareGlobalConnectivity(SqliteDataService service)
        {
            if (service != null)
            {
                _connectionString = null;
                _commandTimeout = service.CommandTimeout;
                _autoRefreshConnection = false;
                GlobalConnection = service.GlobalConnection;      // sets _useGlobalConnectivity
                GlobalTransaction = service.GlobalTransaction;    // sets _useTransaction
                _performOrder = service.PerformOrder;
                _forceRollbackGlobalTransaction = service.ForceRollbackGlobalTransaction;
            }
        }

        #endregion
    }
}
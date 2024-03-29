﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using FluidFramework.Context;
using FluidFramework.Data;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Data
{
    /// <summary>
    /// Inherited by a data service that works with MySQL server.
    /// </summary>
    public partial class MySqlDataService : Component
    {
        #region Properties

        /// <summary>
        /// Protected member for connection string used to create the connection to the MySQL server.
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
        protected MySqlConnection _globalConnection;

        /// <summary>
        /// Protected member for the global transaction object.
        /// </summary>
        protected MySqlTransaction _globalTransaction;

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
        /// Connection string used to create the connection to the MySQL server.        
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
        public MySqlConnection GlobalConnection
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
        public MySqlTransaction GlobalTransaction
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
        public MySqlDataService()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public MySqlDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// Helper property to create a new DataService instance.
        /// </summary>
        public static MySqlDataService New
        {
            get
            {
                return new MySqlDataService();
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
        public MySqlDataService GlobalInitialize(MySqlConnection connection, MySqlTransaction transaction, int? commandTimeout,
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
        public MySqlDataService ServiceInitialize(MySqlDataService service)
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
        protected void CheckAffectedRows(MySqlAdapterConfiguration ac, int affectedRows)
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
        protected void PerformLoop(List<MySqlAdapterConfiguration> listAdapterConfiguration, bool isUpdate)
        {
            string sqlTableName = null;
            try
            {
                for (int index = 0; index < listAdapterConfiguration.Count; index++)
                {
                    sqlTableName = null;
                    MySqlAdapterConfiguration ac = listAdapterConfiguration[isUpdate ? index : listAdapterConfiguration.Count - 1 - index];
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
        public void Perform(List<MySqlAdapterConfiguration> listAdapterConfiguration)
        {
            MySqlConnection sqlConnection = null;
            MySqlTransaction sqlTransaction = null;

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
                foreach (MySqlAdapterConfiguration ac in listAdapterConfiguration)
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
                foreach (MySqlAdapterConfiguration ac in listAdapterConfiguration)
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
        public void Perform(MySqlAdapterConfiguration adapterConfiguration)
        {
            Perform(new List<MySqlAdapterConfiguration> { adapterConfiguration });
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
        /// Creates a new MySQL connection with the given connection string or with the connection string from context.
        /// </summary>
        protected virtual MySqlConnection CreateConnection()
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
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Opens the given MySQL connection.
        /// </summary>
        protected MySqlConnection OpenConnection(MySqlConnection sqlConnection = null)
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
        /// Opens a transactional MySQL connection and returns the connection and the transaction.
        /// </summary>
        protected MySqlConnection OpenTransactionalConnection(MySqlConnection sqlConnection, out MySqlTransaction sqlTransaction)
        {
            // Connection
            sqlConnection = OpenConnection(sqlConnection);

            // Transaction
            sqlTransaction = sqlConnection.BeginTransaction();
            
            return sqlConnection;
        }

        /// <summary>
        /// Closes the given MySQL connection.
        /// </summary>
        protected void CloseConnection(MySqlConnection sqlConnection)
        {
            if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
        }

        /// <summary>
        /// Commits and frees the given MySQL transaction.
        /// </summary>
        protected void CommitTransaction(MySqlTransaction sqlTransaction)
        {
            if (sqlTransaction != null)
            {
                sqlTransaction.Commit();
                sqlTransaction.Dispose();
            }
        }

        /// <summary>
        /// Rolls back and frees the given MySQL transaction.
        /// </summary>
        protected void RollbackTransaction(MySqlTransaction sqlTransaction)
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
        protected void SetParameters(MySqlCommand sqlCommand, List<ParameterInfo> parameterList)
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
        protected void SetCommand(MySqlCommand sqlCommand, MySqlConnection sqlConnection, MySqlTransaction sqlTransaction = null)
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
        protected void UnsetCommand(MySqlCommand sqlCommand)
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
        public void ShareGlobalConnectivity(MySqlDataService service)
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

using System;
using System.Data.SqlClient;
using System.Data;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// This class helps working with the SQL database.
    /// </summary>
    public static class DataUtils
    {
        /// <summary>
        /// A delegate that gives the number of blocks.
        /// </summary>
        public delegate int GetBlockCountDelegate();

        /// <summary>
        /// A delegate that checks if a block is valid.
        /// </summary>
        public delegate bool IsBlockValidDelegate(int index);

        /// <summary>
        /// A delegate that gives the block content.
        /// </summary>
        public delegate string GetBlockDelegate(int index);

        /// <summary>
        /// A delegate that gives the number of repetitions for a block.
        /// </summary>
        public delegate int GetRepeatCountDelegate(int index);

        /// <summary>
        /// A delegate that handles the error of the execution.
        /// </summary>
        public delegate void CustomHandleErrorDelegate(Exception exception, int index);

        /// <summary>
        /// A delegate that updates the progress of the execution.
        /// </summary>
        public delegate void CustomUpdateStateDelegate(int index);

        /// <summary>
        /// Checks if a connection can be estabilished in the given amount of time.
        /// </summary>
        public static bool CheckConnection(string connectionString, int connectionTimeout)
        {
            SqlConnection sqlConnection = null;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString)
                {
                    ConnectTimeout = connectionTimeout
                };
                sqlConnection = new SqlConnection(builder.ConnectionString);
                sqlConnection.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        sqlConnection.Close();
                    }
                    sqlConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a new sql command with the given connection, content and timeout.
        /// </summary>
        public static SqlCommand PrepareCommand(string connectionString, string commandString, int commandTimeout)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = commandString,
                    CommandTimeout = commandTimeout
                };
                return command;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fills a data table inside the given dataset using the sql command.
        /// </summary>
        public static bool FillData(SqlCommand command, string tableName, DataSet dataset)
        {
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter
                {
                    SelectCommand = command,
                    ContinueUpdateOnError = false
                };
                if (dataset.Tables.Contains(tableName))
                {
                    dataset.Tables[tableName].Clear();
                }
                adapter.Fill(dataset, tableName);
                return true;
            }
            catch
            {
                return false;
            }            
        }

        /// <summary>
        /// Checks if a database exists using the given connection.
        /// </summary>
        public static bool CheckDatabase(string connectionString, string databaseName)
        {
            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand
                {
                    Connection = sqlConnection,
                    CommandText = @"SELECT 1 FROM master.sys.databases WHERE name = @dbname",
                    CommandTimeout = 60
                };
                SqlParameter parameter = new SqlParameter("@dbname", SqlDbType.NVarChar,256)
                {
                    Value = databaseName
                };
                command.Parameters.Add(parameter);
                sqlConnection.Open();
                object obj = command.ExecuteScalar();
                if(obj != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks if database is accessible.
        /// </summary>
        public static bool CheckDatabaseIsAccessible(string connectionString)
        {
            try
            {
                DataSet dataSet = new DataSet();
                FillData(PrepareCommand(connectionString, "SELECT name FROM sys.objects WHERE type='U'", 60), "tables", dataSet);
                if (dataSet.Tables["tables"].Select().Length > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void ExecuteScript(SqlConnection sqlConnection, SqlTransaction sqlTransaction,
                                          GetBlockCountDelegate getBlockCount,
                                          IsBlockValidDelegate isBlockValid,
                                          GetBlockDelegate getBlock,
                                          GetRepeatCountDelegate getRepeatCount,
                                          CustomHandleErrorDelegate customHandleError,
                                          CustomUpdateStateDelegate customUpdateState,
                                          int commandTimeout)
        {
            using (SqlCommand cmd = sqlConnection.CreateCommand())
            {
                cmd.Connection = sqlConnection;
                cmd.CommandTimeout = commandTimeout;
                if (sqlTransaction != null)
                {
                    cmd.Transaction = sqlTransaction;
                }
                int blockNumber = getBlockCount();
                for (int index = 0; index < blockNumber; index++)
                {
                    if (isBlockValid(index))
                    {
                        cmd.CommandText = getBlock(index);
                        cmd.CommandType = CommandType.Text;
                        try
                        {
                            int repeatNumber = getRepeatCount(index);
                            for (int count = 1; count <= repeatNumber; count++)
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception exception)
                        {
                            if (customHandleError != null)
                            {
                                customHandleError(exception, index);
                            }
                            throw;                            
                        }
                    }
                    if (customUpdateState != null)
                    {
                        customUpdateState(index);
                    }
                }
            }
        }

        /// <summary>
        /// Executes a Transact-SQL script against the connection specified by the connection string.
        /// </summary>
        public static void ExecuteCustomSqlScript(string connectionString,
            GetBlockCountDelegate getBlockCount,
            IsBlockValidDelegate isBlockValid,
            GetBlockDelegate getBlock,
            GetRepeatCountDelegate getRepeatCount,
            CustomHandleErrorDelegate customHandleError = null,
            CustomUpdateStateDelegate customUpdateState = null,
            bool useTransaction = true,
            int commandTimeout = 3600)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    if (useTransaction)
                    {
                        using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                        {
                            try
                            {
                                ExecuteScript(sqlConnection, sqlTransaction, getBlockCount, isBlockValid, getBlock,
                                              getRepeatCount, customHandleError, customUpdateState, commandTimeout);

                                sqlTransaction.Commit();
                            }
                            catch
                            {
                                sqlTransaction.Rollback();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        ExecuteScript(sqlConnection, null, getBlockCount, isBlockValid, getBlock,
                                      getRepeatCount, customHandleError, customUpdateState, commandTimeout);
                    }                    
                }
                finally
                {
                    if (sqlConnection.State != ConnectionState.Broken && sqlConnection.State != ConnectionState.Closed)
                    {
                        sqlConnection.Close();
                    }                   
                }
            }
        }
    }
}

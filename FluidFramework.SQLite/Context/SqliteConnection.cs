﻿using System;
using System.Data;
using System.Data.SQLite;
using FluidFramework.Context;

namespace FluidFramework.SQLite.Context
{
    /// <summary>
    /// Stores information for a SQLite connection.
    /// </summary>
    public class SqliteConnection : Connection
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SqliteConnection() { }

        /// <summary>
        /// A constructor that takes the verbatim connection string.
        /// </summary>
        public SqliteConnection(string connectionString) : base(connectionString) { }

        /// <summary>
        /// A constructor that takes the verbatim connection string and the SQL command timeout.
        /// </summary>
        public SqliteConnection(string connectionString, int commandTimeout) : base(connectionString, commandTimeout) { }

        /// <summary>
        /// A constructor that takes the connection parameters.
        /// </summary>
        public SqliteConnection(string databaseFile, string password, int defaultTimeout = 30)
        {
            try
            {
                SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder
                {
                    DataSource = databaseFile,
                    DefaultTimeout = defaultTimeout,
                    FailIfMissing = true,
                    ForeignKeys = true
                };
                if (!String.IsNullOrEmpty(password))
                {
                    csb.Password = password;
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
        public SqliteConnection(string databaseFile, string password, int defaultTimeout, int commandTimeout) : this(databaseFile, password, defaultTimeout)
        {
            CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Obtains the database file from the connection string.
        /// </summary>
        public string GetDatabaseFile()
        {
            try
            {
                if (String.IsNullOrEmpty(ConnectionString))
                {
                    return null;
                }

                SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(ConnectionString);

                return csb.DataSource;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtains the password from the connection string.
        /// </summary>
        public string GetPassword()
        {
            try
            {
                if (String.IsNullOrEmpty(ConnectionString))
                {
                    return null;
                }

                SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(ConnectionString);

                return csb.Password;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtains the connection timeout from the connection string.
        /// </summary>
        public int GetDefaultTimeout()
        {
            try
            {
                if (String.IsNullOrEmpty(ConnectionString))
                {
                    return 30;
                }

                SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(ConnectionString);

                return csb.DefaultTimeout;
            }
            catch
            {
                return 30;
            }
        }

        /// <summary>
        /// Tests if the connection can be opened.
        /// </summary>
        public override bool TestConnection(int preferredConnectionTimeout = 15)
        {
            SQLiteConnection connection = null;
            try
            {
                SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(ConnectionString)
                {
                    DefaultTimeout = preferredConnectionTimeout
                };
                connection = new SQLiteConnection(builder.ToString());
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

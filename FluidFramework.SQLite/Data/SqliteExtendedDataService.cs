using System.ComponentModel;
using System.Data;
using System.Data.SQLite;

namespace FluidFramework.SQLite.Data
{
    /// <summary>
    /// Inherited by a data service that needs extended functionality
    /// </summary>
    public partial class SqliteExtendedDataService : SqliteDataService
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqliteExtendedDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public SqliteExtendedDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public SqliteExtendedDataService(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            InitializeComponent();
            GlobalInitialize(connection, transaction);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads an empty table from database.
        /// </summary>
        public void CreateTable(DataSet dataset, string tableName)
        {
            //Perform(SqlServerFluidSelector.New(tableName).SetCondition("1=0").Configuration(dataset));
        }

        #endregion
    }
}
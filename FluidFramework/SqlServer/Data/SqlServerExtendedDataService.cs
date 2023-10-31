using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace FluidFramework.SqlServer.Data
{
    /// <summary>
    /// Inherited by a data service that needs extended functionality
    /// </summary>
    public partial class SqlServerExtendedDataService : SqlServerDataService
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlServerExtendedDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public SqlServerExtendedDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public SqlServerExtendedDataService(SqlConnection connection, SqlTransaction transaction)
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
            Perform(SqlServerFluidSelector.New(tableName).SetCondition("1=0").Configuration(dataset));
        }

        #endregion
    }
}

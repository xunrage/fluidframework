using System.ComponentModel;
using System.Data;
using MySql.Data.MySqlClient;

namespace FluidFramework.MySql.Data
{
    /// <summary>
    /// Inherited by a data service that needs extended functionality
    /// </summary>
    public partial class MySqlExtendedDataService : MySqlDataService
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MySqlExtendedDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public MySqlExtendedDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public MySqlExtendedDataService(MySqlConnection connection, MySqlTransaction transaction)
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
            Perform(MySqlFluidSelector.New(tableName).SetCondition("1=0").Configuration(dataset));
        }

        #endregion
    }
}

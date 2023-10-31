using System.ComponentModel;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace FluidFramework.Oracle.Data
{
    /// <summary>
    /// Inherited by a data service that needs extended functionality
    /// </summary>
    public partial class OracleExtendedDataService : OracleDataService
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OracleExtendedDataService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a container.
        /// </summary>
        public OracleExtendedDataService(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with a given global connection and transaction.
        /// </summary>
        public OracleExtendedDataService(OracleConnection connection, OracleTransaction transaction)
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
            Perform(OracleFluidSelector.New(tableName).SetCondition("1=0").Configuration(dataset));
        }

        #endregion
    }
}

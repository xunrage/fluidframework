using System;
using System.Data;
using FluidFramework.Context;

namespace FluidFramework.Data
{
    /// <summary>
    /// Provides utility methods.
    /// </summary>
    public static class DalUtility
    {
        /// <summary>
        /// The audit user column
        /// </summary>
        public static string userColumnDefault = "AuditUser";

        /// <summary>
        /// The audit date column
        /// </summary>
        public static string dateColumnDefault = "AuditDate";

        /// <summary>
        /// The audit work station column
        /// </summary>
        public static string workColumnDefault = "AuditWorkStation";

        private static User GetUser(bool check = true)
        {
            User user = ClientContext.Instance.Properties.CurrentUser;

            if (user == null)
            {
                throw new Exception("Audit failed.");
            }

            if (check)
            {
                if (String.IsNullOrEmpty(user.UserName) || String.IsNullOrEmpty(user.WorkStation))
                {
                    throw new Exception("Audit failed.");
                }
            }

            return user;
        }

        private static (string userColumn, string dateColumn, string workColumn) PrepareColumns(string userColumn, string dateColumn, string workColumn)
        {
            if (userColumn == null)
            {
                userColumn = userColumnDefault;
            }

            if (dateColumn == null)
            {
                dateColumn = dateColumnDefault;
            }

            if (workColumn == null)
            {
                workColumn = workColumnDefault;
            }

            return (userColumn, dateColumn, workColumn);
        }

        /// <summary>
        /// Fills the audit for all the added or modified rows in the given dataset.
        /// </summary>
        public static DataSet AddAudit(DataSet dataset, bool forced = false,
                                       string userColumn = null, string dateColumn = null, string workColumn = null)
        {
            (userColumn, dateColumn, workColumn) = PrepareColumns(userColumn, dateColumn, workColumn);            
            
            GetUser();
            
            foreach (DataTable table in dataset.Tables) AddAudit(table, forced, false, userColumn, dateColumn, workColumn);

            return dataset;
        }

        /// <summary>
        /// Fills the audit for all the added or modified rows in the given data table.
        /// </summary>
        public static DataSet AddAudit(DataTable table, bool forced = false, bool check = true,
                                       string userColumn = null, string dateColumn = null, string workColumn = null)
        {
            (userColumn, dateColumn, workColumn) = PrepareColumns(userColumn, dateColumn, workColumn);

            if (!table.Columns.Contains(userColumn) && !table.Columns.Contains(dateColumn) && !table.Columns.Contains(workColumn)) return table.DataSet;

            User user = GetUser(check);

            foreach (DataRow row in table.Select(null, null, forced ? DataViewRowState.CurrentRows : DataViewRowState.Added | DataViewRowState.ModifiedCurrent))
            {
                if (table.Columns.Contains(userColumn))
                {
                    row[userColumn] = user.UserName;
                }
                if (table.Columns.Contains(dateColumn))
                {
                    row[dateColumn] = DateTime.Now;
                }
                if (table.Columns.Contains(workColumn))
                {
                    row[workColumn] = user.WorkStation;
                }
            }

            return table.DataSet;
        }

        /// <summary>
        /// Fills the audit for the given row. It is useful when the row is added.
        /// </summary>        
        public static void AddAudit(DataRow row,
                                    string userColumn = null, string dateColumn = null, string workColumn = null)
        {
            (userColumn, dateColumn, workColumn) = PrepareColumns(userColumn, dateColumn, workColumn);

            User user = GetUser();

            if (row.Table == null)
            {
                try
                {
                    row[userColumn] = user.UserName;
                }
                catch{}
                try
                {
                    row[dateColumn] = DateTime.Now;
                }
                catch{}
                try
                {
                    row[workColumn] = user.WorkStation;
                }
                catch{}
            }
            else
            {
                if (row.Table.Columns.Contains(userColumn))
                {
                    row[userColumn] = user.UserName;
                }
                if (row.Table.Columns.Contains(dateColumn))
                {
                    row[dateColumn] = DateTime.Now;
                }
                if (row.Table.Columns.Contains(workColumn))
                {
                    row[workColumn] = user.WorkStation;
                }
            }
        }        
    }
}
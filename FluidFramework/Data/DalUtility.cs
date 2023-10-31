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
        public static string userColumn = "AuditUser";

        /// <summary>
        /// The audit date column
        /// </summary>
        public static string dateColumn = "AuditDate";

        /// <summary>
        /// The audit work station column
        /// </summary>
        public static string workColumn = "AuditWorkStation";

        /// <summary>
        /// Fills the audit for all the added or modified rows in the given dataset.
        /// </summary>
        public static DataSet AddAudit(DataSet ds, bool forced = false)
        {
            User user = ClientContext.Instance.Properties.CurrentUser;
            if (user == null || String.IsNullOrEmpty(user.UserName) || String.IsNullOrEmpty(user.WorkStation)) throw new Exception("Audit failed.");
            foreach (DataTable dt in ds.Tables) AddAudit(dt, forced, false);
            return ds;
        }

        /// <summary>
        /// Fills the audit for all the added or modified rows in the given data table.
        /// </summary>
        public static DataSet AddAudit(DataTable dt, bool forced = false, bool check = true)
        {
            if (!dt.Columns.Contains(userColumn) && !dt.Columns.Contains(dateColumn) && !dt.Columns.Contains(workColumn)) return dt.DataSet;

            User user = ClientContext.Instance.Properties.CurrentUser;
            if(user == null) throw new Exception("Audit failed.");
            if (check)
            {
                if (String.IsNullOrEmpty(user.UserName) || String.IsNullOrEmpty(user.WorkStation)) throw new Exception("Audit failed.");
            }

            foreach (DataRow row in dt.Select(null, null, forced ? DataViewRowState.CurrentRows : DataViewRowState.Added | DataViewRowState.ModifiedCurrent))
            {
                if (dt.Columns.Contains(userColumn))
                {
                    row[userColumn] = user.UserName;
                }
                if (dt.Columns.Contains(dateColumn))
                {
                    row[dateColumn] = DateTime.Now;
                }
                if (dt.Columns.Contains(workColumn))
                {
                    row[workColumn] = user.WorkStation;
                }
            }
            return dt.DataSet;
        }

        /// <summary>
        /// Fills the audit for the given row. It is useful when the row is added.
        /// </summary>        
        public static void AddAudit(DataRow row)
        {
            User user = ClientContext.Instance.Properties.CurrentUser;
            if (user == null || String.IsNullOrEmpty(user.UserName) || String.IsNullOrEmpty(user.WorkStation)) throw new Exception("Audit failed.");
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
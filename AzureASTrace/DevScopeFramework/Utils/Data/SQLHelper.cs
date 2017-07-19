using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace DevScope.Framework.Common.Utils
{
    public static class SQLHelper
    {
        public static void ExecuteBulkCopy<T>(string connectionString, string tableName, IList<T> data, List<string> tableColumns, Action<DataRow, T> bindTableRow, SqlTransaction transaction = null, int timeout = 30, Action<SqlRowsCopiedEventArgs> rowsCopiedEvent = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                ExecuteBulkCopy<T>(conn, tableName, data, tableColumns, bindTableRow, transaction, timeout, rowsCopiedEvent);
            }
        }

        public static void ExecuteBulkCopy<T>(SqlConnection conn, string tableName, IList<T> data, List<string> tableColumns, Action<DataRow, T> bindTableRow, SqlTransaction transaction = null, int timeout = 30, Action<SqlRowsCopiedEventArgs> rowsCopiedEvent = null)
        {
            using (var table = new DataTable(tableName))
            {
                foreach (var col in tableColumns)
                {
                    table.Columns.Add(col, typeof(object));
                }

                foreach (var item in data)
                {
                    var row = table.NewRow();

                    bindTableRow(row, item);

                    table.Rows.Add(row);
                }
                
                ExecuteBulkCopy(conn, tableName, table, transaction, timeout, rowsCopiedEvent);
            }
        }

        public static void ExecuteBulkCopy(string connectionString, string tableName, object data, SqlTransaction transaction = null, int timeout = 30, Action<SqlRowsCopiedEventArgs> rowsCopiedEvent = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                ExecuteBulkCopy(conn, tableName, data, transaction, timeout, rowsCopiedEvent);
            }
        }

        public static void ExecuteBulkCopy(SqlConnection connection, string tableName, object data, SqlTransaction transaction = null, int timeout = 30, Action<SqlRowsCopiedEventArgs> rowsCopiedEvent = null)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");
            if (data == null)
                throw new ArgumentNullException("data");

            //var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction);
            using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulk.DestinationTableName = tableName;
                bulk.BulkCopyTimeout = timeout;

                if (data is DataTable)
                {
                    var dt = (DataTable)data;

                    foreach (DataColumn col in dt.Columns)
                    {
                        bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                }

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                if (rowsCopiedEvent != null)
                {
                    bulk.NotifyAfter = 1000;

                    bulk.SqlRowsCopied += (sender, args) =>
                        {
                            rowsCopiedEvent(args);
                        };
                }

                if (data is IDataReader)
                {
                    bulk.WriteToServer((IDataReader)data);
                }
                else if (data is DataTable)
                {
                    bulk.WriteToServer((DataTable)data);
                }
                else
                {
                    throw new ApplicationException(string.Format("Invalid data type: '{0}'", data.GetType().Namespace));
                }
            }
        }

    }
}
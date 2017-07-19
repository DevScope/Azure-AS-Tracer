using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DevScope.Framework.Common.Utils
{
    public static partial class SQLHelper
    {
        public static async Task ExecuteBulkCopyAsync(SqlConnection connection, string tableName, object data, SqlTransaction transaction = null, int timeout = 30, Action<SqlRowsCopiedEventArgs> rowsCopiedEvent = null)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");
            if (data == null)
                throw new ArgumentNullException("data");
            
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
                    await connection.OpenAsync();
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
                    await bulk.WriteToServerAsync((IDataReader)data);
                }
                else if (data is DataTable)
                {
                    await bulk.WriteToServerAsync((DataTable)data);
                }
                else
                {
                    throw new ApplicationException(string.Format("Invalid data type: '{0}'", data.GetType().Namespace));
                }
            }
        }
    }
}
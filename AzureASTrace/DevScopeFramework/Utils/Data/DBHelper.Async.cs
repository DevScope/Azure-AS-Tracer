using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using DevScope.Framework.Common.Extensions;
using System.Threading.Tasks;
using System.Dynamic;

namespace DevScope.Framework.Common.Utils
{
    public static partial class DBHelper
    {        
        public static async Task<object> ExecuteCommandAsync(string connectionString, string command, CommandResultTypeEnum commandResultType = CommandResultTypeEnum.Table, CommandType commandType = CommandType.Text, IEnumerable<DbParameter> parameters = null, string providerName = "System.Data.SqlClient", Action<DbCommand> onCreateCommand = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (var conn = GetConnection(connectionString, providerName))
            {
                return await ExecuteCommandAsync(conn, command, commandResultType, commandType, parameters, onCreateCommand);
            }
        }

        public static async Task<object> ExecuteCommandAsync(DbConnection conn, string commandText
            , CommandResultTypeEnum commandResultType = CommandResultTypeEnum.Table, CommandType commandType = CommandType.Text
            , IEnumerable<DbParameter> parameters = null, Action<DbCommand> onCreateCommand = null)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");

            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            using (var cmd = GetCommand(conn, commandText, commandType, (parameters != null ? parameters.ToArray() : null)))
            {
                if (onCreateCommand != null)
                {
                    onCreateCommand(cmd);
                }
                
                object result = null;                

                switch (commandResultType)
                {
                    case CommandResultTypeEnum.Table:
                        {
                            DataTable table = null;

                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    // Inicializar DataTable

                                    if (table == null)
                                    {
                                        table = new DataTable();

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            table.Columns.Add(reader.GetName(i), typeof(object));
                                        }
                                    }

                                    // Criar linha na datatable

                                    var row = table.NewRow();

                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        var obj = await reader.GetFieldValueAsync<object>(i);

                                        row[i] = obj;
                                    }

                                    table.Rows.Add(row);
                                }
                            }

                            result = table;
                        }
                        break;
                    case CommandResultTypeEnum.NonQuery:
                        {
                            result = await cmd.ExecuteNonQueryAsync();
                        }
                        break;
                    case CommandResultTypeEnum.Reader:
                        {
                            result = await cmd.ExecuteReaderAsync();
                        }
                        break;
                    default:
                        throw new ApplicationException(string.Format("Invalid ExecuteCommand result type: '{0}'", commandResultType));

                }

                if (cmd.Parameters != null)
                {
                    cmd.Parameters.Clear();
                }

                return result;
            }
        }
      
        public static async Task<List<T>> ExecuteQueryAsync<T>(string connectionString, string commandText
            , Func<Dictionary<string, object>, T> bind
            , CommandType commandType = CommandType.Text
            , IEnumerable<DbParameter> parameters = null
            , Action<DbCommand> onCreateCommand = null
            , string providerName = "System.Data.SqlClient"
            )
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (var conn = GetConnection(connectionString, providerName))
            {
                return await ExecuteQueryAsync(conn, commandText, bind, commandType, parameters, onCreateCommand);
            }
        }

        public static async Task<List<T>> ExecuteQueryAsync<T>(DbConnection conn, string commandText
            , Func<Dictionary<string, object>, T> bind = null
            , CommandType commandType = CommandType.Text
            , IEnumerable<DbParameter> parameters = null
            , Action<DbCommand> onCreateCommand = null)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");
                        
            var result = new List<T>();

            using (var reader = (DbDataReader)await ExecuteCommandAsync(conn
                , commandText: commandText
                , commandResultType: CommandResultTypeEnum.Reader
                , parameters: parameters
                , onCreateCommand: onCreateCommand                
                ))
            {
                var columnsDictionary = new Dictionary<string, object>();

                while (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var key = reader.GetName(i);

                        columnsDictionary[key] = (await reader.GetFieldValueAsync<object>(i)).IgnoreDBNull();
                    }

                    T obj = default(T);

                    if (bind != null)
                    {
                        obj = bind(columnsDictionary);  
                    }
                    else
                    {
                        obj = (dynamic)columnsDictionary.ToExpando();
                    }

                    result.Add(obj);      
                }
            }

            return result;
        }
    }
}

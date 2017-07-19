using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using DevScope.Framework.Common.Extensions;
using System.Dynamic;

namespace DevScope.Framework.Common.Utils
{
    public static class DBHelper
    {
        public static T ExecuteCommand<T>(string connectionString, string command, CommandType commandType = CommandType.Text, IEnumerable<DbParameter> parameters = null, string providerName = "System.Data.SqlClient", Action<IDbCommand> onCreateCommand = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            if (typeof(T) == typeof(IDataReader))
            {
                throw new ApplicationException("DataReader not supported with 'connectionString' parameter");
            }

            using (var conn = GetConnection(connectionString, providerName))
            {
                return ExecuteCommand<T>(conn, command, commandType, parameters, onCreateCommand);
            }
        }

        public static T ExecuteCommand<T>(IDbConnection conn, string commandText, CommandType commandType = CommandType.Text, IEnumerable<DbParameter> parameters = null, Action<IDbCommand> onCreateCommand = null)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");

            using (var cmd = GetCommand(conn, commandText, commandType, (parameters != null ? parameters.ToArray() : null)))
            {
                if (onCreateCommand != null)
                {
                    onCreateCommand(cmd);
                }

                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                object result = null;

                var type = typeof(T);

                if (type == typeof(DataSet) || type == typeof(DataTable))
                {
                    var ds = new DataSet();

                    var factory = GetProviderFactory(conn.GetType().Namespace);                    

                    using (var adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = (DbCommand)cmd;

                        adapter.Fill(ds);
                    }

                    if (type == typeof(DataSet))
                    {
                        result = ds;
                    }
                    else
                    {
                        result = ds.Tables[0];
                    }
                }
                else if (type == typeof(IDataReader))
                {
                    result = cmd.ExecuteReader();
                }
                else if (type == typeof(int))
                {
                    result = cmd.ExecuteNonQuery();
                }             
                else if (type == typeof(List<dynamic>))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        result = reader.ToList();
                    }
                }   
                else
                {
                    throw new ApplicationException(string.Format("Invalid ExecuteCommand result type: '{0}'", type.Name));
                }

                if (cmd.Parameters != null)
                {
                    cmd.Parameters.Clear();
                }

                return (T)result;
            }
        }

        public static DbConnection GetConnection(string connectionString, string providerName = "System.Data.SqlClient")
        {
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException("providerName");
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            var factory = GetProviderFactory(providerName);

            var conn = factory.CreateConnection();

            conn.ConnectionString = connectionString;

            return conn;
        }

        public static IDbCommand GetCommand(IDbConnection connection, string commandText, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");

            var cmd = connection.CreateCommand();

            cmd.CommandText = commandText;

            cmd.CommandType = commandType;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }

            return cmd;
        }

        public static List<T> ToList<T>(this IDataReader reader, string columnName)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            var result = new List<T>();

            while (reader.Read())
            {
                result.Add((T)reader[columnName]);
            }

            return result;
        }

        public static List<dynamic> ToList(this IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return reader.ToList<dynamic>(isDynamic: true);
        }

        public static List<T> ToList<T>(this IDataReader reader, Action<IDataReader, T> setProperties = null, Func<IDataReader, T> customBind = null, bool isDynamic = false)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            var result = new List<T>();

            if (customBind != null)
            {
                while (reader.Read())
                {
                    result.Add(customBind(reader));
                }
            }
            else
            {
                var columns = new List<dynamic>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(new { Ordinal = i, Name = reader.GetName(i) });
                }

                if (isDynamic)
                {
                    while (reader.Read())
                    {
                        var obj = new ExpandoObject();

                        var objDic = (ICollection<KeyValuePair<string, object>>)obj;

                        foreach (var column in columns)
                        {
                            var columnName = (string)column.Name;
                            var columnOrdinal = (int)column.Ordinal;

                            var propertyValue = reader.GetValue(columnOrdinal);

                            objDic.Add(new KeyValuePair<string, object>(columnName, propertyValue.IgnoreDBNull())); 
                        }

                        result.Add((T)((object)obj));
                    }
                }
                else
                {
                    var type = typeof(T);

                    var sampleProperties = type.GetProperties();

                    while (reader.Read())
                    {
                        var obj = (T)Activator.CreateInstance(typeof(T));

                        foreach (var column in columns)
                        {
                            var columnName = (string)column.Name;
                            var columnOrdinal = (int)column.Ordinal;

                            var property = sampleProperties.SingleOrDefault(s => s.Name.EqualsCI(columnName));

                            if (property == null)
                            {
                                continue;
                            }

                            var propertyValue = reader.GetValue(columnOrdinal);

                            property.SetValue(obj, propertyValue.IgnoreDBNull(), null);
                        }

                        if (setProperties != null)
                        {
                            setProperties(reader, obj);
                        }

                        result.Add(obj);
                    }
                }
            }

            return result;
        }

        #region Private Methods

        private static DbProviderFactory GetProviderFactory(string name)
        {            
            return DbProviderFactories.GetFactory(name);
        }

        #endregion
    }
}

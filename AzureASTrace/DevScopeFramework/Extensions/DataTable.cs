using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DevScope.Framework.Common.Extensions
{
    public static class DataTableExtensions
    {
        public static DataRow ImportRow2(this DataTable table, DataRow row)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (row == null)
                throw new ArgumentNullException("row");

            table.ImportRow(row);

            return table.Rows[table.Rows.Count - 1];
        }
        public static DataTable RemoveColumns(this DataTable table, params string[] columns)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (columns == null)
                throw new ArgumentNullException("columns");

            var columnsToRemove = table.Columns.Cast<DataColumn>().Where(s => columns.Any(s2 => s2.EqualsCI(s.ColumnName))).ToList();

            foreach (var col in columnsToRemove)
            {
                table.Columns.Remove(col);
            }

            return table;
        }

        public static DataTable FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");

            var ds = new DataSet();

            using (var sr = new StringReader(xml))
            {
                ds.ReadXml(sr);
            }

            return ds.Tables[0];
        }

        public static string ToXml(this DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                table.WriteXml(sw);
            }

            return sb.ToString();
        }

        public static DataTable CopyToDataTableFromDynamic(this IEnumerable<object> list, string tableName = null, List<string> columnsToCreate = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            DataTable table = null;

            PropertyInfo[] dynamicObjProperties = null;

            var notAllColumns = columnsToCreate != null && columnsToCreate.Count != 0;

            // Criar linhas

            foreach (var obj in list)
            {
                // Na primeira iteração cria as colunas da tabela (deve ser feito aqui senão a query Linq dispara 2x)

                if (table == null)
                {
                    table = new DataTable();
                    table.TableName = tableName;

                    var firstElement = obj;

                    dynamicObjProperties = firstElement.GetType().GetProperties();

                    // Criar colunas com base no tipo do 1º elemento

                    foreach (var prop in dynamicObjProperties)
                    {

                        if (notAllColumns && !columnsToCreate.Contains(prop.Name))
                        {
                            continue;
                        }
                        var type = prop.PropertyType;

                        if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            var nullableConverter = new NullableConverter(type);

                            type = nullableConverter.UnderlyingType;
                        }

                        table.Columns.Add(prop.Name, type);
                    }
                }

                var row = table.NewRow();

                foreach (var prop in dynamicObjProperties)
                {
                    var colName = prop.Name;
                    if (notAllColumns && !columnsToCreate.Contains(colName))
                    {
                        continue;
                    }

                    object value;
                    
                    value = (object)prop.GetValue(obj, null);
                    

                    row[colName] = value.AsDBNull();
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Igual ao original, mas não dá erro quando a collection é vazia
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static System.Data.DataTable CopyToDataTable2(this IEnumerable<System.Data.DataRow> source, string tableName = null)
        {
            //// Se não tiver nenhuma linha então devolve NULL    
            //if (!source.Any())
            //{
            //    return null;
            //}
            //else
            //{
            DataTable table;
            try
            {
                table = System.Data.DataTableExtensions.CopyToDataTable<System.Data.DataRow>(source);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            table.TableName = tableName;

            return table;
            //}
        }

        public static T GetValue<T>(this DataRow row, int colIndex)
        {
            if (row == null)
                throw new ArgumentNullException("row");
            
            return ParseValue<T>(row[colIndex]);
        }

        public static T GetValue<T>(this DataRow row, string columnName)
        {
            if (row == null)
                throw new ArgumentNullException("row");
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");            

            return ParseValue<T>(row[columnName]);
        }

        private static T ParseValue<T>(object value)
        {
            value = value.IgnoreDBNull();

            if (value == null)
                return default(T);            

            var type = typeof(T);

            // Checks if is Nullable, and if it is get the nullable underlying type

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var nullableConverter = new NullableConverter(type);

                type = nullableConverter.UnderlyingType;
            }

            if (type == typeof(object))
            {
                return (T)value;
            }

            if (type == typeof(DateTime) && value is double)
            {
                return (T)(object)DateTime.FromOADate((double)value);
            }

            return (T)Convert.ChangeType(value, type);
        }

        //public static string ToJSON(DataTable table)
        //{
        //    if (table == null)
        //        throw new ArgumentNullException("table");

        //    JavaScriptSerializer serializer = new JavaScriptSerializer();

        //    //Usa list intermédia
        //    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        //    Dictionary<string, object> row = null;

        //    foreach (DataRow dr in table.Rows)
        //    {
        //        row = new Dictionary<string, object>();
        //        foreach (DataColumn col in table.Columns)
        //        {
        //            row.Add(col.ColumnName, dr[col]);
        //        }
        //        rows.Add(row);
        //    }
        //    return serializer.Serialize(rows);

        //}
    }
}

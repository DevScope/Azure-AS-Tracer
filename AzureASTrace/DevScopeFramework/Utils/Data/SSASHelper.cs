using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AnalysisServices.AdomdClient;
using System.Xml;
using System.Data;
using System.Xml.Linq;
using System.ComponentModel;
using System.Configuration;
using DevScope.Framework.Common.Logging;
using DevScope.Framework.Common.Utils;
using DevScope.Framework.Common.Extensions;

namespace DevScope.Framework.Common.Utils
{
    /// <summary>
    /// Defines an OLAPManager.
    /// </summary>
    public static class SSASHelper
    {
        #region Public Methods

        public static DataTable ExecuteQueryAsDataTable(string connectionString, string query, params AdomdParameter[] parameters)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (AdomdConnection conn = GetConnection(connectionString))
            {
                return ExecuteCommand<DataTable>(conn, query, parameters);
            }
        }

        /// <summary>
        /// Executes the query as cell set.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static CellSet ExecuteQueryAsCellSet(string connectionString, string query, params AdomdParameter[] parameters)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (AdomdConnection conn = GetConnection(connectionString))
            {
                return ExecuteQueryAsCellSet(conn, query, parameters);
            }
        }

        /// <summary>
        /// Executes the query as cell set.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static CellSet ExecuteQueryAsCellSet(AdomdConnection conn, string query, params AdomdParameter[] parameters)
        {
            return ExecuteCommand<CellSet>(conn, query, parameters);
        }


        /// <summary>
        /// Executes the query as adomd data reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static AdomdDataReader ExecuteQueryAsAdomdDataReader(AdomdConnection conn, string query, params AdomdParameter[] parameters)
        {
            return ExecuteCommand<AdomdDataReader>(conn, query, parameters);
        }

        /// <summary>
        /// Executes the xmla.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="xmla">The xmla.</param>
        /// <param name="parameters">The parameters.</param>     
        /// <returns></returns>
        public static string ExecuteXmla(string connectionString, string xmla, params AdomdParameter[] parameters)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (AdomdConnection conn = GetConnection(connectionString))
            {
                return ExecuteXmla(conn, xmla, parameters);
            }
        }

        /// <summary>
        /// Executes the xmla.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="xmla">The xmla.</param>
        /// <param name="parameters">The parameters.</param>        
        /// <returns></returns>
        public static string ExecuteXmla(AdomdConnection conn, string xmla, params AdomdParameter[] parameters)
        {
            string xmlaResult;

            // To read the result the connection must be opened

            using (var reader = (XmlReader)ExecuteCommand<XmlReader>(conn, xmla, parameters))
            {
                xmlaResult = reader.ReadOuterXml();
            }

            // Check for errors

            if (string.IsNullOrEmpty(xmlaResult))
            {
                throw new ApplicationException("XmlaResult is null or empty.");
            }

            var xmlaResultXml = XDocument.Parse(xmlaResult);

            var nsEx = XNamespace.Get("urn:schemas-microsoft-com:xml-analysis:exception");

            // If there's an exception in the xmla result, then throw an exception with the error

            var exceptionElem = xmlaResultXml.Descendants(nsEx + "Exception").FirstOrDefault();

            if (exceptionElem != null) // if there's an exception then fail the task
            {
                // Get all the errors and throw exception

                var errorsStr = string.Join(System.Environment.NewLine
                                        , exceptionElem.Parent.Descendants(nsEx + "Error").Select(e => e.Attribute("Description").Value));

                throw new ApplicationException(errorsStr);
            }

            return xmlaResult;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <typeparam name="T">Supported types: XmlReader, CellSet, AdomdDataReader or Int</typeparam>
        /// <param name="conn">The conn.</param>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An XmlReader, CellSet, AdomdDataReader or Int</returns>
        public static T ExecuteCommand<T>(AdomdConnection conn, string command, params AdomdParameter[] parameters)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException("command");

            LogQuery(command, parameters);

            using (AdomdCommand cmd = GetCommand(conn, command, parameters))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                object result = null;

                var type = typeof(T);

                if (type == typeof(XmlReader))
                    result = cmd.ExecuteXmlReader();
                else if (type == typeof(CellSet))
                {
                    result = cmd.ExecuteCellSet();
                }
                else if (type == typeof(AdomdDataReader))
                    result = cmd.ExecuteReader();
                else if (type == typeof(int))
                    result = cmd.ExecuteNonQuery();
                else if (type == typeof(DataTable))
                {
                    // Outputs all members of a parent-child hierarchy in a single table column in the flattened result (http://msdn.microsoft.com/en-us/library/ms186627.aspx)
                    cmd.Properties.Add("DbpropMsmdFlattened2", true);

                    using (var adapter = new AdomdDataAdapter(cmd))
                    {
                        var ds = new DataSet();

                        adapter.Fill(ds);

                        if (ds.Tables.Count != 0)
                        {
                            result = ds.Tables[0];
                        }
                    }
                }
                else
                    throw new ApplicationException(string.Format("Invalid ExecuteCommand result type: '{0}'", type.Name));

                return (T)result;
            }
        }

        private static void LogQuery(string query, AdomdParameter[] parameters)
        {
            var queryPars = new StringBuilder(query);

            if (parameters != null && parameters.Length != 0)
            {
                var keys = parameters.Select(p => p.ParameterName).OrderBy(p => p).Reverse();

                foreach (string key in keys)
                {
                    object value = parameters.First(p => p.ParameterName == key).Value;

                    queryPars.Replace(string.Format("@{0}", key), string.Format("'{0}'", value));
                }
            }

            Logger.Debug("Executing SSAS Query: '{0}'", queryPars.ToString());
        }

        #endregion

        #region RowSet schemas

        public class SSAS_RowSets
        {
            public static readonly Guid DBSCHEMA_CATALOGS = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Catalogs;
            public static readonly Guid MDSCHEMA_CUBES = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Cubes;
            public static readonly Guid MDSCHEMA_DIMENSIONS = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Dimensions;
            public static readonly Guid MDSCHEMA_HIERARCHIES = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Hierarchies;
            public static readonly Guid MDSCHEMA_MEASURES = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Measures;
            public static readonly Guid MDSCHEMA_MEASUREGROUP_DIMENSIONS = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.MeasureGroupDimensions;
            public static readonly Guid MDSCHEMA_LEVELS = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Levels;
            public static readonly Guid MDSCHEMA_MEMBERS = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.Members;
            public static readonly Guid DISCOVER_SCHEMA_ROWSETS = Microsoft.AnalysisServices.AdomdClient.AdomdSchemaGuid.SchemaRowsets;
        }

        public static DataTable GetSchemaRowset(string connectionString, Guid schemaGuid, string rowFilter = null, params object[] restrictions)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (var conn = new AdomdConnection(connectionString))
            {
                return GetSchemaRowset(conn, schemaGuid, rowFilter, restrictions);
            }
        }

        public static DataTable GetSchemaRowset(AdomdConnection conn, Guid schemaGuid, string rowFilter = null, params object[] restrictions)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (schemaGuid == null)
                throw new ArgumentNullException("schemaGuid");

            // Open the connection if not open


            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            var dt = conn.GetSchemaDataSet(schemaGuid, restrictions).Tables[0];

            //Apply row filter & return the filtered table

            if (!string.IsNullOrEmpty(rowFilter))
            {
                dt.DefaultView.RowFilter = rowFilter;

                dt = dt.DefaultView.ToTable();
            }

            return dt;
        }

        #endregion

        #region Cube Metadata

        public class CubeMetadata
        {
            public CubeMetadata()
            {
                this.Measures = new DataTable();
                this.Measures.Columns.Add("Name", typeof(string));
                this.Measures.Columns.Add("UniqueName", typeof(string));
                this.Measures.Columns.Add("MeasureGroupName", typeof(string));
                this.Measures.Columns.Add("MeasureDisplayFolder", typeof(string));
                
                this.Dimensions = new DataTable();
                this.Dimensions.Columns.Add("Name", typeof(string));
                this.Dimensions.Columns.Add("UniqueName", typeof(string));
                this.Dimensions.Columns.Add("Type", typeof(string));
                this.Dimensions.Columns.Add("Cardinality", typeof(int));
                this.Dimensions.Columns.Add("MeasureGroupName", typeof(string));

                this.Hierarchies = new DataTable();
                this.Hierarchies.Columns.Add("Name", typeof(string));
                this.Hierarchies.Columns.Add("UniqueName", typeof(string));
                this.Hierarchies.Columns.Add("Type", typeof(string));
                this.Hierarchies.Columns.Add("DimensionUniqueName", typeof(string));                

                this.Levels = new DataTable();
                this.Levels.Columns.Add("Name", typeof(string));
                this.Levels.Columns.Add("UniqueName", typeof(string));
                this.Levels.Columns.Add("Type", typeof(string));
                this.Levels.Columns.Add("HierarchyUniqueName", typeof(string));
                this.Levels.Columns.Add("Number", typeof(int));
                this.Levels.Columns.Add("Cardinality", typeof(int));
            }

            public string CubeName { get; set; }
            public DataTable Measures { get; set; }
            public DataTable Dimensions { get; set; }
            public DataTable Hierarchies { get; set; }
            public DataTable Levels { get; set; }

        }

        //public static CubeMetadata GetCubeMetadataWithCache(string connectionString, string cubeName)
        //{
        //    if (string.IsNullOrEmpty(connectionString))
        //        throw new ArgumentNullException("connectionString");

        //    var cacheKey = "{0}.{1}".FormatText(connectionString, cubeName);

        //    var metadata = (CubeMetadata)CacheHelper.Get(cacheKey);

        //    if (metadata == null)
        //    {
        //        metadata = GetCubeMetadata(connectionString, cubeName);

        //        CacheHelper.Set(cacheKey, metadata);
        //    }

        //    return metadata;
        //}  

        public static CubeMetadata GetCubeMetadata(string connectionString, string cubeName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            using (var conn = new AdomdConnection(connectionString))
            {
                return GetCubeMetadata(conn, cubeName);
            }
        }  

        public static CubeMetadata GetCubeMetadata(AdomdConnection conn, string cubeName)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrEmpty(cubeName))
                throw new ArgumentNullException("cubeName");

            var cubeMetadata = new CubeMetadata();

            cubeMetadata.CubeName = SSASHelper.RemoveBrackets(cubeName);

            // Measures

            var dtMeasures = GetSchemaRowset(conn, SSAS_RowSets.MDSCHEMA_MEASURES,
               "[MEASURE_IS_VISIBLE]", null, null, cubeName);

            foreach (var dr in dtMeasures.AsEnumerable())
            {
                var newRow = cubeMetadata.Measures.NewRow();

                newRow["Name"] = dr.Field<string>("MEASURE_CAPTION");

                newRow["UniqueName"] = dr.Field<string>("MEASURE_UNIQUE_NAME");

                newRow["MeasureGroupName"] = dr.Field<string>("MEASUREGROUP_NAME");

                newRow["MeasureDisplayFolder"] = dr.Field<string>("MEASURE_DISPLAY_FOLDER");

                cubeMetadata.Measures.Rows.Add(newRow);
            }

            // Dimensions

            var dtDimensions = GetSchemaRowset(conn, SSAS_RowSets.MDSCHEMA_DIMENSIONS
                , null, null, null, cubeName).AsEnumerable().ToList();

            // Dimensions vs MG relations

            var listDimensionsAndMG = GetSchemaRowset(conn, SSAS_RowSets.MDSCHEMA_MEASUREGROUP_DIMENSIONS
                                        , string.Empty, null, null, cubeName)
                                        .AsEnumerable().ToList();

            foreach (var dr in dtDimensions)
            {
                var newRow = cubeMetadata.Dimensions.NewRow();

                newRow["Name"] = dr.Field<string>("DIMENSION_CAPTION");

                var dimensionUniqueName = dr.Field<string>("DIMENSION_UNIQUE_NAME");

                newRow["UniqueName"] = dimensionUniqueName;

                var type = dr.Field<short>("DIMENSION_TYPE");

                // 2: Measure dimension
                if (type == 2)
                {
                    continue;
                }

                newRow["Type"] = type.ToString();                

                var measureGroups = listDimensionsAndMG.Where(m => m.Field<string>("DIMENSION_UNIQUE_NAME").EqualsCI(dimensionUniqueName))
                    .Select(s => s.Field<string>("MEASUREGROUP_NAME"))
                    .Distinct();

                newRow["MeasureGroupName"] = string.Join(",", measureGroups);

                cubeMetadata.Dimensions.Rows.Add(newRow);
            }

            // Hierarchies

            var dtHierarchies = GetSchemaRowset(conn, SSAS_RowSets.MDSCHEMA_HIERARCHIES
                , "[HIERARCHY_IS_VISIBLE] and [HIERARCHY_UNIQUE_NAME] <> '[Measures]'", null, null, cubeName).AsEnumerable().ToList();


            foreach (var hRow in dtHierarchies.AsEnumerable())
            {
                var newRow = cubeMetadata.Hierarchies.NewRow();

                newRow["Name"] = hRow.Field<string>("HIERARCHY_CAPTION");

                newRow["UniqueName"] = hRow.Field<string>("HIERARCHY_UNIQUE_NAME");

                newRow["Type"] = Convert.ToInt32(hRow["HIERARCHY_ORIGIN"].IgnoreDBNull()) == 1
                    ? "User"
                    : "Attribute";                

                newRow["DimensionUniqueName"] = hRow.Field<string>("DIMENSION_UNIQUE_NAME");

                cubeMetadata.Hierarchies.Rows.Add(newRow);
            }

            // Levels            

            var dtLevels = GetSchemaRowset(conn, SSAS_RowSets.MDSCHEMA_LEVELS
                , "[LEVEL_IS_VISIBLE] and [HIERARCHY_UNIQUE_NAME] <> '[Measures]'", null, null, cubeName).AsEnumerable().ToList();

            foreach (var dr in dtLevels)
            {
                var newRowLevel = cubeMetadata.Levels.NewRow();

                newRowLevel["Name"] = dr.Field<string>("LEVEL_CAPTION");

                newRowLevel["UniqueName"] = dr.Field<string>("LEVEL_UNIQUE_NAME");

                newRowLevel["Type"] = dr.Field<int>("LEVEL_TYPE").ToString();

                newRowLevel["HierarchyUniqueName"] = dr.Field<string>("HIERARCHY_UNIQUE_NAME");

                newRowLevel["Number"] = dr.Field<uint>("LEVEL_NUMBER");

                newRowLevel["Cardinality"] = dr.Field<uint>("LEVEL_CARDINALITY");

                cubeMetadata.Levels.Rows.Add(newRowLevel);
            }


            return cubeMetadata;
        }
   
        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The command.</returns>
        private static AdomdCommand GetCommand(AdomdConnection connection, string commandText, params AdomdParameter[] parameters)
        {
            var cmd = connection.CreateCommand();

            cmd.CommandText = commandText;

            // TODO: read from configuration

            cmd.CommandTimeout = 0;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }

            return cmd;
        }

        public static AdomdConnection GetConnection(string dataSource, string database)
        {
            if (string.IsNullOrEmpty(dataSource))
                throw new ArgumentNullException("dataSource");

            var connStr = new StringBuilder(string.Format("Data Source={0}", dataSource));

            if (!string.IsNullOrEmpty(database))
            {
                connStr.AppendFormat("; Initial Catalog={0}", database);
            }

            return GetConnection(connStr.ToString());
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The connection.</returns>
        public static AdomdConnection GetConnection(string connectionString)
        {
            return new AdomdConnection(connectionString);
        }

        #endregion

        #region Extensions

        public static object GetMemberPropertyValue(this Member member, string propertyName, bool assertExists = true)
        {
            var mp = member.GetMemberProperty(propertyName, assertExists);

            return (mp != null ? mp.Value : null);
        }

        public static MemberProperty GetMemberProperty(this Member member, string propertyName, bool assertExists = true)
        {
            if (member == null)
                throw new ArgumentNullException("member");
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            var mp = member.MemberProperties[propertyName];

            if (mp == null && assertExists)
            {
                throw new ApplicationException(string.Format("Cannot find OLAP member property: '{0}'", propertyName));
            }

            return mp;
        }

        #endregion

        #region Helpers

        public static string RemoveBrackets(this string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            return name.Replace("[", "").Replace("]", "");
        }

        public static string EscapeMDXString(this string mdxStr)
        {
            if (string.IsNullOrEmpty(mdxStr))
                throw new ArgumentNullException("mdxStr");

            var sb = new StringBuilder(mdxStr);

            var escapeChars = new string[] { "'", "]" };

            foreach (var escapeChar in escapeChars)
            {
                sb.Replace(escapeChar, string.Format("{0}{0}", escapeChar));
            }

            return sb.ToString();
        }

        //TEMP
        public static string ParseFullHierarchyNameFromUniqueName(this string uniqueName)
        {

            if (uniqueName == null)
                return "";

            return string.Format("{0} ({1})",
                uniqueName.Split('.')[1].Replace("[", "").Replace("]", ""),
                uniqueName.Split('.')[0].Replace("[", "").Replace("]", ""));
        }

        public static string ParseLevelNameFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            // TODO: Review...

            var args = uniqueName.Split('.');

            if (args.Length > 2)
            {
                return RemoveBrackets(args[2]);
            }
            else
            {
                // Parent Childs: [Employees].[Employee]
                return RemoveBrackets(args[1]);
            }
        }

        public static string ParseHierarchyNameFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            return RemoveBrackets(uniqueName.Split('.')[1]);
        }

        public static string ParseDimensionNameFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            return RemoveBrackets(uniqueName.Split('.')[0]);
        }

        public static string ParseHierarchyFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            var array = uniqueName.Split('.');

            return string.Format("{0}.{1}", array[0], array[1]);
        }

        public static string ParseLevelFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            // Exclude key if exists
            int indexOfKey = uniqueName.IndexOf(".&");
            string field = string.Empty;
            if (indexOfKey > -1)
                field = uniqueName.Substring(0, indexOfKey);
            else
                field = uniqueName;

            string[] fields = field.Split('.');
            if (fields.Length == 3)
                // already a level
                return field;
            else
            {
                // duplicate hierarchy as level
                field += "." + fields[1];
                return field;
            }
        }

        public static string ParseFieldFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            // Exclude key if exists
            int indexOfKey = uniqueName.IndexOf(".&");
            string field = string.Empty;
            if (indexOfKey > -1)
                field = uniqueName.Substring(0, indexOfKey);
            else
                field = uniqueName;

            return field;
        }

        public static string ParseAttributeNameFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            string attributeName = string.Empty;

            // Exclude key if exists
            int indexOfKey = uniqueName.IndexOf(".&");
            string field = string.Empty;
            if (indexOfKey > -1)
                field = uniqueName.Substring(0, indexOfKey);
            else
                field = uniqueName;

            string[] fields = field.Split('.');
            attributeName = fields[fields.Length - 1].Replace("[", "").Replace("]", "");

            return attributeName;
        }

        public static string ParseKeyFromUniqueName(this string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
                throw new ArgumentNullException("uniqueName");

            // Exclude key if exists
            int indexOfKey = uniqueName.IndexOf(".&");
            string field = string.Empty;
            if (indexOfKey > -1)
            {
                string[] fields = uniqueName.Split('&');
                field = fields[fields.Length - 1].Replace("[", "").Replace("]", "");
            }


            return field;
        }

        #endregion
    }
}

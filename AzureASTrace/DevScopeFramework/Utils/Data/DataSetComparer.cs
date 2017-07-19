using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DevScope.Framework.Common.Extensions;

namespace DevScope.Framework.Common.Utils
{
    public static class DataSetComparer
    {
        public static DataSet Compare(DataSet source, DataSet dest)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            DataSet changes = null;

            try
            {
                changes = CreateChangesDataset();

                PrepareDataset(source, changes);

                PrepareDataset(dest, changes);

                foreach (DataTable srcTable in source.Tables)
                {
                    if (dest.Tables.Contains(srcTable.TableName))
                    {
                        DataTable destTable = dest.Tables[srcTable.TableName];
                        CompareTableData(changes, srcTable, destTable);
                    }
                    else
                    {
                        Log(changes, "Table removed: {0}", srcTable.TableName);
                    }
                }

                foreach (DataTable destTable in dest.Tables)
                {
                    if (!source.Tables.Contains(destTable.TableName))
                    {
                        Log(changes, "Table added: {0}", destTable.TableName);
                    }
                }
            }
            catch (Exception ex)
            {
                if (changes != null)
                {
                    Log(changes, "Error: {0}", ex.ToString());
                }
            }           

            return changes;
        }

        private static void PrepareDataset(DataSet dataset, DataSet changes)
        {
            AssertOrCreatePrimaryKeys(dataset, changes);

            AddMetaDataTables(dataset);
        }

        private static void AddMetaDataTables(DataSet ds)
        {
            DataTable metaDataTable = new DataTable("$Tables");
            metaDataTable.Columns.Add("TableName", typeof(string));
            metaDataTable.Columns.Add("Rows", typeof(int));
            metaDataTable.Columns.Add("Columns", typeof(int));
            metaDataTable.PrimaryKey = new DataColumn[] { metaDataTable.Columns["TableName"] };

            foreach (DataTable table in ds.Tables)
            {
                metaDataTable.Rows.Add(new object[] { table.TableName, table.Rows.Count, table.Columns.Count });
            }
            ds.Tables.Add(metaDataTable);
        }

        private static void Log(DataSet ds, string message, params object[] args)
        {
            DataTable logTable = ds.Tables["$Log"];

            DataRow row = logTable.Rows.Add(new object[] { string.Format(message, args) });

        }

        private static void AssertOrCreatePrimaryKeys(DataSet dsSource, DataSet changes)
        {
            foreach (DataTable table in dsSource.Tables)
            {

                Log(changes, "Setting PK on Table {0}", table.TableName);

                if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
                {
                    if (!FindTablePrimaryKey(table))
                    {
                        continue;
                        //throw new ApplicationException("No primary key candidate found for table: " + table.TableName);
                    }
                    else
                    {
                        var pkCol = table.PrimaryKey[0];

                        pkCol.SetOrdinal(1);
                    }
                }
            }
        }

        private static bool FindTablePrimaryKey(DataTable table)
        {
            // SysPK is a system pk
            if (table.Columns.Contains("sysPK"))
            {
                table.PrimaryKey = new DataColumn[] { table.Columns["sysPK"] };

                return true;
            }
            else
            {
                int totalRows = table.Rows.Count;

                //TODO: poucos registos podem resultar em falsos positivos de pks
                foreach (DataColumn col in table.Columns)
                {
                    int distinctCount = table.AsEnumerable().Select(x => x[col]).Distinct().Count();
                    if (totalRows == distinctCount)
                    {
                        table.PrimaryKey = new DataColumn[] { col };
                        return true;
                    }
                }
            }

            return false;
        }    

        private static DataSet CreateChangesDataset()
        {
            DataSet changes = new DataSet();

            DataTable logTable = new DataTable("$Log");
            logTable.Columns.Add("Message", typeof(string));

            changes.Tables.Add(logTable);

            return changes;
        }

        private static void CompareTableData(DataSet changes, DataTable table1, DataTable table2)
        {
            Log(changes, "Comparing data for table:{0}", table1.TableName);

            if (table1.PrimaryKey.Length == 0|| table2.PrimaryKey.Length == 0)
            {
                Log(changes, "WARNING: Table '{0}' dont have a Primary Key", table1.TableName);
                return;
            }

            DataTable changeTable = new DataTable(table1.TableName);
            changeTable.Columns.Add("$Status", typeof(string));

            changes.Tables.Add(changeTable);

            string pkCol = table1.PrimaryKey[0].ColumnName;

            foreach (DataColumn col in table1.Columns)
            {
                changeTable.Columns.Add(col.ColumnName, typeof(string));
            }

            // Compare Columns

            foreach (DataColumn col in table1.Columns)
            {
                if (!table2.Columns.Contains(col.ColumnName))
                {
                    Log(changes, "Missing column '{0}' in table '{1}'.", col.ColumnName, table2.TableName);

                    continue;
                }
            }

            int changedRows = 0, newRows = 0, deletedRows = 0;

            foreach (DataRow newRow in table2.Rows)
            {
                DataRow oldRow = table1.Rows.Find(newRow[pkCol]);
                DataRow difRow = null;

                if (oldRow == null)
                {
                    //new row
                    difRow = changeTable.NewRow();
                    difRow[pkCol] = newRow[pkCol];

                    foreach (DataColumn col in table1.Columns)
                    {                        
                        if (!table2.Columns.Contains(col.ColumnName))
                        {                            
                            continue;
                        } 

                        difRow[col.ColumnName] = newRow[col.ColumnName];
                    }

                    difRow["$Status"] = "New";
                    
                    newRows++;
                }
                else
                {
                    //row exists, compare
                    foreach (DataColumn col in table1.Columns)
                    {
                        if (!table2.Columns.Contains(col.ColumnName))
                        {                            
                            continue;
                        }

                        var newValue = newRow[col.ColumnName];
                        var oldValue = oldRow[col.ColumnName];

                        var newValueStr = newValue + "";
                        var oldValueStr = oldValue + "";

                        // Round

                        if (oldValue.IsNumeric())
                        {
                            oldValueStr = string.Format("{0:#,#.###}", oldValue);
                        }

                        if (newValue.IsNumeric())
                        {
                            newValueStr = string.Format("{0:#,#.###}", newValue);
                        }

                        if (!newValueStr.Equals(oldValueStr, StringComparison.OrdinalIgnoreCase))
                        {                            
                            if (difRow == null)
                            {
                                difRow = changeTable.NewRow();
                                difRow[pkCol] = newRow[pkCol];
                                changedRows++;
                            }

                            var value = string.Format("{0} ==> {1}", oldValueStr, newValueStr);
                            
                            difRow[col.ColumnName] = value;

                            difRow["$Status"] = "Changed";
                        }
                    }
                }

                if (difRow != null)
                {
                    changeTable.Rows.Add(difRow);
                }
            }


            //deleted rows
            foreach (DataRow oldRow in table1.Rows)
            {
                DataRow newRow = table2.Rows.Find(oldRow[pkCol]);

                if (newRow == null)
                {
                    DataRow row = changeTable.NewRow();
                    row[pkCol] = oldRow[pkCol];
                    foreach (DataColumn col in table1.Columns)
                    {
                        row[col.ColumnName] = oldRow[col.ColumnName];
                    }

                    row["$Status"] = "Deleted";
                    changeTable.Rows.Add(row);
                    deletedRows++;
                }

            }

            Log(changes, "Stats for table:{0} Rows: {4} New:{1} Changed:{2} Deleted:{3}", table1.TableName, newRows, changedRows, deletedRows, table1.Rows.Count);

        }

    }
}

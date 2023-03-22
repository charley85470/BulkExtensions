using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BulkExtensions
{
    internal class SqlBulkUtil
    {
        public SqlBulkUtil()
        {

        }

        /// <summary>
        /// 大批新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="tableInfo"></param>
        /// <param name="entities"></param>
        public static void BulkInsert<T>(IDatabaseHandler handler, TableInfo tableInfo, IList<T> entities) where T : class
        {
            try
            {
                DataTable dt = GetDataTable(tableInfo, entities);
                // 排除自增Key
                foreach (var storeGeneratedKey in tableInfo.PrimaryKeys.Where(x => x.IsStoreGenerated).Select(x => x.Name))
                {
                    dt.Columns.Remove(storeGeneratedKey);
                }
                handler.OpenConnection();
                BulkCopy(handler, dt, tableInfo.TableName);
            }
            finally
            {
                handler.CloseConnection();
            }
        }

        /// <summary>
        /// 大批更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="tableInfo"></param>
        /// <param name="entities"></param>
        public static void BulkUpdate<T>(IDatabaseHandler handler, TableInfo tableInfo, IList<T> entities) where T : class
        {
            try
            {
                handler.OpenConnection();
                DataTable dt = GetDataTable(tableInfo, entities);
                handler.ExecuteSql(SqlBuilder.DropTempTable(tableInfo));
                handler.ExecuteSql(SqlBuilder.CreateTempTable(tableInfo));
                BulkCopy(handler, dt, tableInfo.TempTableName);
                handler.ExecuteSql(SqlBuilder.Merge(tableInfo, tableInfo.TempTableName, SqlBuilder.MergeType.Update));
            }
            finally
            {
                handler.CloseConnection();
            }
        }

        /// <summary>
        /// 大批刪除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="tableInfo"></param>
        /// <param name="entities"></param>
        public static void BulkDelete<T>(IDatabaseHandler handler, TableInfo tableInfo, IList<T> entities) where T : class
        {
            try
            {
                handler.OpenConnection();
                DataTable dt = GetDataTable(tableInfo, entities);
                handler.ExecuteSql(SqlBuilder.DropTempTable(tableInfo));
                handler.ExecuteSql(SqlBuilder.CreateTempTable(tableInfo));
                BulkCopy(handler, dt, tableInfo.TempTableName);
                handler.ExecuteSql(SqlBuilder.Merge(tableInfo, tableInfo.TempTableName, SqlBuilder.MergeType.Delete));
            }
            finally
            {
                handler.CloseConnection();
            }
        }

        internal static DataTable GetDataTable<T>(TableInfo tableInfo, IList<T> entities)
        {
            DataTable dt = new DataTable();
            foreach (var column in tableInfo.Columns)
            {
                dt.Columns.Add(column);
            }

            foreach (var item in entities)
            {
                DataRow dr = dt.NewRow();

                foreach (DataColumn column in dt.Columns)
                {
                    dr[column.ColumnName] =
                        typeof(T).GetProperty(column.ColumnName).GetValue(item) ?? DBNull.Value;
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        internal static void BulkCopy(IDatabaseHandler handler, DataTable dataTable, string destinationTableName)
        {
            var sqlBulkCopy = new SqlBulkCopy(
                (SqlConnection)handler.Connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)handler.Transaction)
            {
                DestinationTableName = destinationTableName,
                //設定一個批次量寫入多少筆資料
                BatchSize = 1000,
                //設定逾時的秒數
                BulkCopyTimeout = 60
            };
            //對應資料行
            foreach (DataColumn column in dataTable.Columns)
            {
                sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            sqlBulkCopy.WriteToServer(dataTable);
        }
    }
}

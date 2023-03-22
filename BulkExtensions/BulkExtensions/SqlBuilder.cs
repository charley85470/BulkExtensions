using System;
using System.Collections.Generic;
using System.Text;

namespace BulkExtensions
{
    public class SqlBuilder
    {
        public enum MergeType
        {
            /// <summary>
            /// 更新
            /// </summary>
            Update,
            /// <summary>
            /// 刪除
            /// </summary>
            Delete
        }

        /// <summary>
        /// 取得Merge語法
        /// </summary>
        /// <param name="targetTableInfo">目標資料表資訊</param>
        /// <param name="sourceTableName">來源資料表名稱</param>
        /// <param name="mergeType">Match時的處理方式</param>
        /// <returns>SQL語法</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string Merge(TableInfo targetTableInfo, string sourceTableName, MergeType mergeType)
        {
            string sql = $"MERGE {targetTableInfo.TableName} T " +
                         $"USING {sourceTableName} S ON {GetConditions(targetTableInfo, "T", "S")} " +
                         $"WHEN MATCHED THEN ";

            switch (mergeType)
            {
                case MergeType.Update:
                    return sql + $"UPDATE SET {GetUpdateSetCommand(targetTableInfo, "T", "S")};";
                case MergeType.Delete:
                    return sql + $"DELETE;";
                default:
                    throw new ArgumentException("Parameter mergeType not valid.", "mergeType");
            }
        }

        /// <summary>
        /// 移除暫存表
        /// </summary>
        /// <param name="tableInfo">資料表資訊</param>
        /// <returns>SQL語法</returns>
        public static string DropTempTable(TableInfo tableInfo)
        {
            return $"IF OBJECT_ID(N'tempdb..[{tableInfo.TempTableName}]', N'U') IS NOT NULL DROP TABLE {tableInfo.TempTableName}";
        }

        /// <summary>
        /// 建立暫存表
        /// </summary>
        /// <param name="tableInfo">資料表資訊</param>
        /// <returns>SQL語法</returns>
        public static string CreateTempTable(TableInfo tableInfo)
        {
            return $"SELECT TOP (0) {GetColumnsCommaSeperated(tableInfo.Columns)} INTO {tableInfo.TempTableName} FROM {tableInfo.TableName}";
        }

        /// <summary>
        /// 取得對應條件
        /// </summary>
        /// <param name="tableInfo">目標資料表資訊</param>
        /// <param name="targetAlias">目標資料表別名</param>
        /// <param name="sourceAlias">來源資料表別名</param>
        /// <returns>SQL語法</returns>
        private static string GetConditions(TableInfo tableInfo, string targetAlias, string sourceAlias)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyInfo primaryKey in tableInfo.PrimaryKeys)
            {
                sb.Append($" AND {targetAlias}.{primaryKey.Name}={sourceAlias}.{primaryKey.Name}");
            }
            return sb.ToString().Substring(5);
        }

        /// <summary>
        /// 取得更新資料對應
        /// </summary>
        /// <param name="tableInfo">目標資料表資訊</param>
        /// <param name="targetAlias">目標資料表別名</param>
        /// <param name="sourceAlias">來源資料表別名</param>
        /// <returns>SQL語法</returns>
        private static string GetUpdateSetCommand(TableInfo tableInfo, string targetAlias, string sourceAlias)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string column in tableInfo.GetNotPrimaryKeyColumns())
            {
                sb.Append($",{targetAlias}.{column}={sourceAlias}.{column}");
            }
            return sb.ToString().Substring(1);
        }

        /// <summary>
        /// 欄位逗號分隔
        /// </summary>
        /// <param name="columns">欄位列表</param>
        /// <returns>SQL語法</returns>
        private static string GetColumnsCommaSeperated(IList<string> columns)
        {
            return string.Join(",", columns);
        }
    }
}

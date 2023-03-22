using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace BulkExtensions
{
    public class TableInfo
    {
        /// <summary>
        /// 索引鍵
        /// </summary>
        public IList<KeyInfo> PrimaryKeys { get; set; }
        /// <summary>
        /// 欄位
        /// </summary>
        public IList<string> Columns { get; set; }
        /// <summary>
        /// 資料表名稱
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 暫存表名稱
        /// </summary>
        public string TempTableName { get { return $"#{TableName}"; } }

        /// <summary>
        /// 實體化物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static TableInfo CreatInstance<T>(DbContext dbContext) where T : class
        {
            return new TableInfo
            {
                TableName = typeof(T).Name,
                Columns = dbContext.GetColumns<T>(),
                PrimaryKeys = dbContext.GetPrimaryKeys<T>()
            };
        }

        /// <summary>
        /// 取得非索引鍵欄位
        /// </summary>
        /// <returns></returns>
        public IList<string> GetNotPrimaryKeyColumns()
        {
            return Columns.Except(PrimaryKeys.Select(x => x.Name)).ToList();
        }
    }
}

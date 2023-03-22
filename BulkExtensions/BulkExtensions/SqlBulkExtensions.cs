using System.Collections.Generic;
using System.Data.Entity;

namespace BulkExtensions
{
    public static class SqlBulkExtensions
    {
        /// <summary>
        /// 大批新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entities"></param>
        public static void BulkInsert<T>(this DbContext dbContext, IList<T> entities) where T : class
        {
            var tableInfo = TableInfo.CreatInstance<T>(dbContext);
            IDatabaseHandler handler = new DbContextHandler(dbContext);
            SqlBulkUtil.BulkInsert(handler, tableInfo, entities);
        }

        /// <summary>
        /// 大批更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entities"></param>
        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> entities) where T : class
        {
            var tableInfo = TableInfo.CreatInstance<T>(dbContext);
            IDatabaseHandler handler = new DbContextHandler(dbContext);
            SqlBulkUtil.BulkUpdate(handler, tableInfo, entities);
        }

        /// <summary>
        /// 大批刪除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entities"></param>
        public static void BulkDelete<T>(this DbContext dbContext, IList<T> entities) where T : class
        {
            var tableInfo = TableInfo.CreatInstance<T>(dbContext);
            IDatabaseHandler handler = new DbContextHandler(dbContext);
            SqlBulkUtil.BulkDelete(handler, tableInfo, entities);
        }
    }
}

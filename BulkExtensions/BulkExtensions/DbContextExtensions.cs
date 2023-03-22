using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace BulkExtensions
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// 取得資料表所有欄位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static IList<string> GetColumns<T>(this DbContext dbContext) where T : class
        {
            ObjectContext objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            return objectContext.CreateObjectSet<T>().EntitySet.ElementType.Properties.Select(x => x.Name).ToList();
        }

        /// <summary>
        /// 取得資料表索引鍵欄位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static IList<KeyInfo> GetPrimaryKeys<T>(this DbContext dbContext) where T : class
        {
            ObjectContext objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            var properties = objectContext.CreateObjectSet<T>().EntitySet.ElementType.KeyProperties;
            return properties.Select(x => new KeyInfo
            {
                Name = x.Name,
                IsStoreGenerated = x.MetadataProperties
                    .Any(y => y.Name.Contains("StoreGeneratedPattern") && (string)y.Value == "Identity")
            }).ToList();
        }
    }
}

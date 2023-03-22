using BulkExtensions;
using BulkExtensionsTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace BulkExtensionsTests
{
    [TestClass]
    public class SqlBulkExtensionsTest
    {
        DateTime now;

        [TestInitialize]
        public void Init()
        {
            now = DateTime.Now;
            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                testDBEntities.Database.ExecuteSqlCommand(@"
                    DELETE Scheme;
                    DELETE Product; 
                    DELETE SchemeType; 
                    DBCC CHECKIDENT('SchemeType', RESEED, 0);
                ");

                testDBEntities.Product.RemoveRange(testDBEntities.Product);
                List<Product> products = new List<Product>()
                {
                    new Product { ProductId = "1", Name = "Product 1", Price = (decimal)100.15, CreateDate = now },
                    new Product { ProductId = "2", Name = "Product 2", Price = (decimal)250.32, CreateDate = now },
                    new Product { ProductId = "3", Name = "Product 3", Price = (decimal)2.5, CreateDate = now },
                };

                List<SchemeType> schemeTypes = new List<SchemeType>() {
                    new SchemeType { Name = "TypeA" },
                    new SchemeType { Name = "TypeB" },
                    new SchemeType { Name = "TypeC" },
                };

                List<Scheme> schemes = new List<Scheme>
                {
                    new Scheme { SchemeId = "S1", ProductId = "1", SchemeTypeId = 1, SchemeName = "SCH1 1" },
                    new Scheme { SchemeId = "S2", ProductId = "1", SchemeTypeId = 3, SchemeName = "SCH2 1" },
                    new Scheme { SchemeId = "S3", ProductId = "3", SchemeTypeId = 2, SchemeName = "SCH3 3" },
                };

                testDBEntities.Product.AddRange(products);
                testDBEntities.SchemeType.AddRange(schemeTypes);
                testDBEntities.SaveChanges();
                testDBEntities.Scheme.AddRange(schemes);
                testDBEntities.SaveChanges();
            }
        }

        [TestMethod]
        public void BulkInsertTest()
        {
            List<Product> insertProducts = new List<Product>()
            {
                new Product { ProductId = "4", Name = "Product 4", Price = (decimal)100.15, CreateDate = now },
                new Product { ProductId = "5", Name = "Product 5", Price = (decimal)250.32, CreateDate = now },
                new Product { ProductId = "6", Name = "Product 6", Price = (decimal)2.5, CreateDate = now },
            };

            List<SchemeType> insertSchemeTypes = new List<SchemeType>()
            {
                new SchemeType { Name = "Type1" },
                new SchemeType { Name = "Type2" },
                new SchemeType { Name = "Type3" },
            };

            List<Scheme> insertSchemes = new List<Scheme>
            {
                new Scheme { SchemeId = "S4", ProductId = "4", SchemeTypeId = 1, SchemeName = "SCH 4" },
                new Scheme { SchemeId = "S5", ProductId = "5", SchemeTypeId = 3, SchemeName = "SCH 5" },
                new Scheme { SchemeId = "S6", ProductId = "6", SchemeTypeId = 2, SchemeName = "SCH 6" },
            };

            using (TransactionScope ts = new TransactionScope())
            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                testDBEntities.BulkInsert(insertProducts);
                foreach (var expect in insertProducts)
                {
                    var actual = testDBEntities.Product.First(x => x.ProductId == expect.ProductId);
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expect.Name, actual.Name);
                    Assert.AreEqual(expect.Price, actual.Price);
                    Assert.AreEqual(expect.CreateDate.ToString("yyyyMMddHHmmss"), actual.CreateDate.ToString("yyyyMMddHHmmss"));
                }

                testDBEntities.BulkInsert(insertSchemeTypes);
                foreach (var expect in insertSchemeTypes)
                {
                    var actual = testDBEntities.SchemeType.FirstOrDefault(a => a.Name == expect.Name);  // 因為是自增主鍵所以只能用Name取
                    Assert.IsNotNull(actual);
                    Assert.IsTrue(actual.SchemeTypeId > 0);
                }

                testDBEntities.BulkInsert(insertSchemes);
                foreach (var expect in insertSchemes)
                {
                    var actual = testDBEntities.Scheme.FirstOrDefault(a => a.SchemeId == expect.SchemeId && a.ProductId == expect.ProductId);
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(actual.SchemeName, actual.SchemeName);
                    Assert.AreEqual(actual.SchemeTypeId, actual.SchemeTypeId);
                }
            }
        }

        [TestMethod]
        public void BulkUpdateTest()
        {
            List<Product> updateProducts = new List<Product>()
            {
                new Product { ProductId = "2", Name = "Product 8", Price = (decimal)130.5, CreateDate = now },
                new Product { ProductId = "3", Name = "Product 9", Price = (decimal)140.3, CreateDate = now },
            };

            List<SchemeType> updateSchemeTypes = new List<SchemeType>()
            {
                new SchemeType { SchemeTypeId = 1, Name = "TypeX" },
                new SchemeType { SchemeTypeId = 2, Name = "TypeY" }
            };

            List<Scheme> updateSchemes = new List<Scheme>
            {
                new Scheme { SchemeId = "S1", ProductId = "1", SchemeTypeId = 2, SchemeName = "SCH 2 1" },
                new Scheme { SchemeId = "S2", ProductId = "1", SchemeTypeId = 2, SchemeName = "SCH 2 2" },
            };

            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                testDBEntities.BulkUpdate(updateProducts);
                foreach (var expect in updateProducts)
                {
                    var actual = testDBEntities.Product.First(x => x.ProductId == expect.ProductId);
                    Assert.AreEqual(expect.Name, actual.Name);
                    Assert.AreEqual(expect.Price, actual.Price);
                }

                testDBEntities.BulkUpdate(updateSchemeTypes);
                foreach (var expect in updateSchemeTypes)
                {
                    var actual = testDBEntities.SchemeType.First(x => x.SchemeTypeId == expect.SchemeTypeId);
                    Assert.AreEqual(expect.Name, actual.Name);
                }

                testDBEntities.BulkUpdate(updateSchemes);
                foreach (var expect in updateSchemes)
                {
                    var actual = testDBEntities.Scheme.First(x => x.SchemeId == expect.SchemeId);
                    Assert.AreEqual(expect.SchemeName, actual.SchemeName);
                    Assert.AreEqual(expect.SchemeTypeId, actual.SchemeTypeId);
                }
            }
        }

        [TestMethod]
        public void BulkDeleteTest()
        {
            List<Product> deleteProducts = new List<Product>()
            {
                new Product { ProductId = "2", Name = "Product 2", Price = (decimal)250.32, CreateDate = now },
                new Product { ProductId = "3", Name = "Product 3", Price = (decimal)2.5, CreateDate = now },
            };

            List<SchemeType> deleteSchemeTypes = new List<SchemeType>() {
                new SchemeType { SchemeTypeId = 2 },
            };

            List<Scheme> deleteSchemes = new List<Scheme>
            {
                new Scheme { SchemeId = "S3", ProductId = "3", SchemeTypeId = 2 },
            };

            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                int productBeforeDelCntProduct = testDBEntities.Product.Count();
                int schemeTypeBeforeDelCntProduct = testDBEntities.SchemeType.Count();
                int schemeBeforeDelCntProduct = testDBEntities.Scheme.Count();
                testDBEntities.BulkDelete(deleteSchemes);       // 有Reference所以要先刪除
                testDBEntities.BulkDelete(deleteSchemeTypes);
                testDBEntities.BulkDelete(deleteProducts);
                Assert.AreEqual(testDBEntities.Product.Count(), productBeforeDelCntProduct - deleteProducts.Count);
                Assert.AreEqual(testDBEntities.SchemeType.Count(), schemeTypeBeforeDelCntProduct - deleteSchemeTypes.Count);
                Assert.AreEqual(testDBEntities.Scheme.Count(), schemeBeforeDelCntProduct - deleteSchemes.Count);

                foreach (var expect in deleteProducts)
                {
                    Assert.IsNull(testDBEntities.Product.FirstOrDefault(x => x.ProductId == expect.ProductId));
                }

                foreach (var expect in deleteSchemeTypes)
                {
                    Assert.IsNull(testDBEntities.SchemeType.FirstOrDefault(x => x.SchemeTypeId == expect.SchemeTypeId));
                }

                foreach (var expect in deleteSchemes)
                {
                    Assert.IsNull(testDBEntities.Scheme.FirstOrDefault(x => x.SchemeId == expect.SchemeId && x.ProductId == expect.ProductId));
                }
            }
        }

        [TestMethod]
        public void GetPrimaryKeysTest()
        {
            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                // Product => 一般字串主鍵
                var productKeys = testDBEntities.GetPrimaryKeys<Product>();
                Assert.IsTrue(productKeys.Count == 1);
                List<KeyInfo> expectProductKeys = new List<KeyInfo>
                {
                    new KeyInfo { Name = "ProductId", IsStoreGenerated = false }
                };
                foreach (var expect in expectProductKeys)
                {
                    var actual = productKeys.FirstOrDefault(x => x.Name == expect.Name);
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expect.IsStoreGenerated, actual.IsStoreGenerated);
                }

                // Scheme => 組合主鍵
                var schemeKeys = testDBEntities.GetPrimaryKeys<Scheme>();
                Assert.IsTrue(schemeKeys.Count == 2);
                List<KeyInfo> expectSchemeKeys = new List<KeyInfo>
                {
                    new KeyInfo { Name = "ProductId", IsStoreGenerated = false },
                    new KeyInfo { Name = "SchemeId", IsStoreGenerated = false }
                };
                foreach (var expect in expectSchemeKeys)
                {
                    var actual = schemeKeys.FirstOrDefault(x => x.Name == expect.Name);
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expect.IsStoreGenerated, actual.IsStoreGenerated);
                }

                // SchemeType => 自增int主鍵
                var schemeTypeKeys = testDBEntities.GetPrimaryKeys<SchemeType>();
                Assert.IsTrue(schemeTypeKeys.Count == 1);
                List<KeyInfo> expectSchemeTypeKeys = new List<KeyInfo>
                {
                    new KeyInfo { Name = "SchemeTypeId", IsStoreGenerated = true }
                };
                foreach (var expect in expectSchemeTypeKeys)
                {
                    var actual = schemeTypeKeys.FirstOrDefault(x => x.Name == expect.Name);
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expect.IsStoreGenerated, actual.IsStoreGenerated);
                }
            }
        }

        [TestMethod]
        public void GetTableColumnsTest()
        {
            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                var productColumns = testDBEntities.GetColumns<Product>();
                Assert.IsTrue(productColumns.Count == 4);
                Assert.IsTrue(productColumns.Contains("ProductId"));
                Assert.IsTrue(productColumns.Contains("Name"));
                Assert.IsTrue(productColumns.Contains("Price"));
                Assert.IsTrue(productColumns.Contains("CreateDate"));

                var schemeColumns = testDBEntities.GetColumns<Scheme>();
                Assert.IsTrue(schemeColumns.Count == 4);
                Assert.IsTrue(schemeColumns.Contains("SchemeId"));
                Assert.IsTrue(schemeColumns.Contains("ProductId"));
                Assert.IsTrue(schemeColumns.Contains("SchemeName"));
                Assert.IsTrue(schemeColumns.Contains("SchemeTypeId"));

                var schemeTypeColumns = testDBEntities.GetColumns<SchemeType>();
                Assert.IsTrue(schemeTypeColumns.Count == 2);
                Assert.IsTrue(schemeTypeColumns.Contains("SchemeTypeId"));
                Assert.IsTrue(schemeTypeColumns.Contains("Name"));
            }
        }

        [TestMethod]
        public void BuildMergeUpdateSqlTest()
        {
            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                TableInfo tableInfo = TableInfo.CreatInstance<Product>(testDBEntities);
                string actual = SqlBuilder.Merge(tableInfo, tableInfo.TempTableName, SqlBuilder.MergeType.Update);
                string expected = "MERGE Product T USING #Product S ON T.ProductId=S.ProductId WHEN MATCHED THEN UPDATE SET T.Name=S.Name,T.Price=S.Price,T.CreateDate=S.CreateDate;";
                Assert.AreEqual(expected, actual);

                TableInfo tableInfo2 = TableInfo.CreatInstance<Scheme>(testDBEntities);
                string actual2 = SqlBuilder.Merge(tableInfo2, tableInfo2.TempTableName, SqlBuilder.MergeType.Update);
                string expected2 = "MERGE Scheme T USING #Scheme S ON T.SchemeId=S.SchemeId AND T.ProductId=S.ProductId WHEN MATCHED THEN UPDATE SET T.SchemeName=S.SchemeName,T.SchemeTypeId=S.SchemeTypeId;";
                Assert.AreEqual(expected2, actual2);
            }
        }

        [TestMethod]
        public void BuildMergeDeleteSqlTest()
        {
            using (TestDBEntities testDBEntities = new TestDBEntities())
            {
                TableInfo tableInfo1 = TableInfo.CreatInstance<Scheme>(testDBEntities);
                string actual1 = SqlBuilder.Merge(tableInfo1, tableInfo1.TempTableName, SqlBuilder.MergeType.Delete);
                string expected1 = "MERGE Scheme T USING #Scheme S ON T.SchemeId=S.SchemeId AND T.ProductId=S.ProductId WHEN MATCHED THEN DELETE;";
                Assert.AreEqual(expected1, actual1);

                TableInfo tableInfo2 = TableInfo.CreatInstance<Product>(testDBEntities);
                string actual2 = SqlBuilder.Merge(tableInfo2, tableInfo2.TempTableName, SqlBuilder.MergeType.Delete);
                string expected2 = "MERGE Product T USING #Product S ON T.ProductId=S.ProductId WHEN MATCHED THEN DELETE;";
                Assert.AreEqual(expected2, actual2);

                TableInfo tableInfo3 = TableInfo.CreatInstance<SchemeType>(testDBEntities);
                string actual3 = SqlBuilder.Merge(tableInfo3, tableInfo3.TempTableName, SqlBuilder.MergeType.Delete);
                string expected3 = "MERGE SchemeType T USING #SchemeType S ON T.SchemeTypeId=S.SchemeTypeId WHEN MATCHED THEN DELETE;";
                Assert.AreEqual(expected3, actual3);
            }
        }
    }
}

using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace BulkExtensions
{
    internal interface IDatabaseHandler
    {
        /// <summary>
        /// 資料庫連線
        /// </summary>
        IDbConnection Connection { get; }
        /// <summary>
        /// 資料庫交易
        /// </summary>
        IDbTransaction Transaction { get; }
        /// <summary>
        /// 開啟連線
        /// </summary>
        void OpenConnection();
        /// <summary>
        /// 關閉連線
        /// </summary>
        void CloseConnection();
        /// <summary>
        /// 執行SQL語法
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void ExecuteSql(string sql, params object[] parameters);
    }

    internal class DbContextHandler : IDatabaseHandler
    {
        private readonly DbContext _dbContext;

        public DbContextHandler(DbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IDbConnection Connection { get => _dbContext.Database.Connection; }

        /// <inheritdoc/>
        public IDbTransaction Transaction
        {
            get
            {
                var transaction = _dbContext.Database.CurrentTransaction;
                return transaction == null ? null : (SqlTransaction)transaction.UnderlyingTransaction;
            }
        }

        /// <inheritdoc/>
        public void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        /// <inheritdoc/>
        public void ExecuteSql(string sql, object[] parameters)
        {
            _dbContext.Database.ExecuteSqlCommand(sql, parameters);
        }

        /// <inheritdoc/>
        public void OpenConnection()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }
    }

}

namespace BulkExtensions
{
    /// <summary>
    /// 主鍵資訊
    /// </summary>
    public class KeyInfo
    {
        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否自動產生
        /// </summary>
        public bool IsStoreGenerated { get; set; }
    }
}

namespace SV22T1020163.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp cơ sở cho các lớp cung cấp dữ liệu trên SQL Server
    /// </summary>
    public abstract class BaseSqlDAL
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        protected string _connectionString = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public BaseSqlDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo và mở một kết nối đến SQL Server
        /// </summary>
        /// <returns></returns>
        protected Microsoft.Data.SqlClient.SqlConnection GetConnection()
        {
            var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}

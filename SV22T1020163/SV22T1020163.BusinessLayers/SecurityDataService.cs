using SV22T1020163.DataLayers.Interfaces;
using SV22T1020163.DataLayers.SqlServer;
using SV22T1020163.Models.Security;

namespace SV22T1020163.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng nghiệp vụ liên quan đến bảo mật (Đăng nhập, Đổi mật khẩu...)
    /// </summary>
    public static class SecurityDataService
    {
        private static readonly IUserAccountRepository userAccountDB;

        /// <summary>
        /// Constructor tĩnh
        /// </summary>
        static SecurityDataService()
        {
            string connectionString = Configuration.ConnectionString;
            userAccountDB = new UserAccountRepository(connectionString);
        }

        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        /// <param name="userName">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>Thông tin tài khoản nếu hợp lệ; ngược lại trả về null</returns>
        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            return await userAccountDB.AuthorizeAsync(userName, password);
        }

        /// <summary>
        /// Thực hiện đổi mật khẩu
        /// </summary>
        /// <param name="userName">Tên tài khoản</param>
        /// <param name="password">Mật khẩu mới</param>
        /// <returns>True nếu thành công</returns>
        public static async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            return await userAccountDB.ChangePasswordAsync(userName, password);
        }
    }
}

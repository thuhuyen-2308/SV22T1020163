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
        /// Đăng nhập nhân viên: so khớp MD5 trước, sau đó thử plain (tài khoản cũ).
        /// </summary>
        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || password == null)
                return null;

            string hash = PasswordHasher.HashMd5(password);
            var user = await userAccountDB.AuthorizeAsync(userName, hash);
            if (user != null)
                return user;

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
            string hashed = string.IsNullOrEmpty(password) ? password ?? "" : PasswordHasher.HashMd5(password);
            return await userAccountDB.ChangePasswordAsync(userName, hashed);
        }
    }
}

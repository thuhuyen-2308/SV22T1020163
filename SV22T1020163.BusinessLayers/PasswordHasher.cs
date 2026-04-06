using System.Security.Cryptography;
using System.Text;

namespace SV22T1020163.BusinessLayers
{
    /// <summary>
    /// Băm mật khẩu MD5 (chuỗi hex 32 ký tự, chữ thường) — UTF-8.
    /// </summary>
    public static class PasswordHasher
    {
        public static string HashMd5(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}

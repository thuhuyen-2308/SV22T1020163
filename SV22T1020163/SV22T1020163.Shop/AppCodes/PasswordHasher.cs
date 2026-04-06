namespace SV22T1020163.Shop.AppCodes;

/// <summary>
/// Bọc gọi tới <see cref="SV22T1020163.BusinessLayers.PasswordHasher"/> để dùng từ Shop (controller/view) mà không trùng logic.
/// </summary>
public static class PasswordHasher
{
    public static string HashMd5(string password) =>
        SV22T1020163.BusinessLayers.PasswordHasher.HashMd5(password);
}

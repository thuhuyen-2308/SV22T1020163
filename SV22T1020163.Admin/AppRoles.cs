namespace SV22T1020163.Admin;

/// <summary>
/// Vai trò khớp cột Employees.RoleNames trong database (admin, manager, sale).
/// </summary>
public static class AppRoles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Sale = "sale";

    /// <summary>Quản trị + quản lý vận hành (không gồm nhân sự hệ thống).</summary>
    public const string AdminManager = Admin + "," + Manager;

    /// <summary>Mọi nhân viên đăng nhập được phép (bán hàng + quản lý).</summary>
    public const string AllStaff = Admin + "," + Manager + "," + Sale;
}

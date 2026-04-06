namespace SV22T1020163.Models.HR
{
    /// <summary>
    /// Nhân viên
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public int EmployeeID { get; set; }
        /// <summary>
        /// Họ và tên
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? BirthDate { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Điện thoại
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Mật khẩu
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// Tên file ảnh (nếu có)
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// Nhân viên đang làm việc hay không?
        /// </summary>
        public bool? IsWorking { get; set; }        
        /// <summary>
        /// Vai trò trong DB (cột RoleNames): admin, manager, sale.
        /// </summary>
        public string RoleNames { get; set; } = string.Empty;
    }
}

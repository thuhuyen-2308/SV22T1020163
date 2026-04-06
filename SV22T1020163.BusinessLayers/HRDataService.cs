using SV22T1020163.DataLayers.Interfaces;
using SV22T1020163.DataLayers.SQLServer;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.HR;

namespace SV22T1020163.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến nhân sự của hệ thống    
    /// </summary>
    public static class HRDataService
    {
        private static readonly IEmployeeRepository employeeDB;

        static HRDataService()
        {
            employeeDB = new EmployeeRepository(Configuration.ConnectionString);
        }

        #region Employee

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhân viên dưới dạng phân trang.
        /// </summary>
        public static async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            return await employeeDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhân viên dựa vào mã nhân viên.
        /// </summary>
        public static async Task<Employee?> GetEmployeeAsync(int employeeID)
        {
            return await employeeDB.GetAsync(employeeID);
        }

        /// <summary>
        /// Bổ sung một nhân viên mới vào hệ thống.
        /// </summary>
        public static async Task<int> AddEmployeeAsync(Employee data)
        {
            if (string.IsNullOrWhiteSpace(data.FullName) || string.IsNullOrWhiteSpace(data.Email))
                throw new ArgumentException("Họ tên và Email không được để trống");

            if (!string.IsNullOrEmpty(data.Password))
                data.Password = PasswordHasher.HashMd5(data.Password);

            return await employeeDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin cơ bản của nhân viên
        /// </summary>
        public static async Task<bool> UpdateEmployeeAsync(Employee data)
        {
            if (string.IsNullOrWhiteSpace(data.FullName))
                return false;

            return await employeeDB.UpdateAsync(data);
        }

        /// <summary>
        /// Cập nhật danh sách quyền của nhân viên
        /// </summary>
        public static async Task<bool> UpdateEmployeeRolesAsync(int employeeID, string[] roleNames)
        {
            var employee = await employeeDB.GetAsync(employeeID);
            if (employee == null) return false;

            // Chuyển mảng thành chuỗi cách nhau bởi dấu phẩy
            employee.RoleNames = (roleNames != null && roleNames.Length > 0)
                                 ? string.Join(",", roleNames)
                                 : "";

            return await employeeDB.UpdateAsync(employee);
        }

        /// <summary>
        /// Danh sách tất cả các quyền định nghĩa trong hệ thống
        /// </summary>
        public static List<string> ListAllRoles()
        {
            return new List<string> { "admin", "employee", "sale", "shipper" };
        }

        /// <summary>
        /// Xóa một nhân viên dựa vào mã nhân viên.
        /// </summary>
        public static async Task<bool> DeleteEmployeeAsync(int employeeID)
        {
            if (await employeeDB.IsUsedAsync(employeeID))
                return false;

            return await employeeDB.DeleteAsync(employeeID);
        }

        /// <summary>
        /// Kiểm tra xem một nhân viên có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        public static async Task<bool> IsUsedEmployeeAsync(int employeeID)
        {
            return await employeeDB.IsUsedAsync(employeeID);
        }

        /// <summary>
        /// Kiểm tra email trùng
        /// </summary>
        public static async Task<bool> ValidateEmployeeEmailAsync(string email, int employeeID = 0)
        {
            return await employeeDB.ValidateEmailAsync(email, employeeID);
        }

        /// <summary>
        /// Đổi mật khẩu nhân viên
        /// </summary>
        public static async Task<bool> ChangePasswordAsync(int employeeID, string newPassword)
        {
            string hashed = string.IsNullOrEmpty(newPassword) ? "" : PasswordHasher.HashMd5(newPassword);
            return await employeeDB.ChangePasswordAsync(employeeID, hashed);
        }

        /// <summary>
        /// Lấy danh sách quyền hiện tại của nhân viên
        /// </summary>
        public static async Task<IList<string>> GetEmployeeRolesAsync(int employeeID)
        {
            return await employeeDB.GetRolesAsync(employeeID);
        }

        #endregion
    }
}
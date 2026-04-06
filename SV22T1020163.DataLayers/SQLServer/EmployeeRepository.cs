using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020163.DataLayers.Interfaces;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.HR;
using System.Data;

namespace SV22T1020163.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu trên nhân viên đối với SQL Server
    /// </summary>
    public class EmployeeRepository : BaseSqlDAL, IEmployeeRepository
    {
        private const string SelectList =
            "EmployeeID, FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public EmployeeRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Thêm mới nhân viên
        /// </summary>
        public async Task<int> AddAsync(Employee data)
        {
            using (var connection = GetConnection())
            {
                var sql = @"INSERT INTO Employees(FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames)
                            VALUES(@FullName, @BirthDate, @Address, @Phone, @Email, @Password, @Photo, @IsWorking, @RoleNames);
                            SELECT SCOPE_IDENTITY();";
                var parameters = new
                {
                    data.FullName,
                    data.BirthDate,
                    data.Address,
                    data.Phone,
                    data.Email,
                    Password = data.Password ?? "",
                    data.Photo,
                    data.IsWorking,
                    RoleNames = string.IsNullOrWhiteSpace(data.RoleNames) ? "sale" : data.RoleNames.Trim()
                };
                var id = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return id;
            }
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = GetConnection())
            {
                var sql = @"DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                var parameters = new { EmployeeID = id };
                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Lấy thông tin một nhân viên
        /// </summary>
        public async Task<Employee?> GetAsync(int id)
        {
            using (var connection = GetConnection())
            {
                var sql = $@"SELECT {SelectList} FROM Employees WHERE EmployeeID = @EmployeeID";
                var parameters = new { EmployeeID = id };
                var data = await connection.QueryFirstOrDefaultAsync<Employee>(sql, parameters);
                return data;
            }
        }

        /// <summary>
        /// Kiểm tra nhân viên có dữ liệu liên quan không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = GetConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Orders WHERE EmployeeID = @EmployeeID)
                                SELECT 1
                            ELSE
                                SELECT 0";
                var parameters = new { EmployeeID = id };
                var result = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return result == 1;
            }
        }

        /// <summary>
        /// Danh sách nhân viên có phân trang và tìm kiếm
        /// </summary>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            using (var connection = GetConnection())
            {
                var sql = @"SELECT COUNT(*) FROM Employees 
                            WHERE (@SearchValue = N'') 
                               OR (FullName LIKE @SearchValue) 
                               OR (Phone LIKE @SearchValue) 
                               OR (Email LIKE @SearchValue)
                               OR (Address LIKE @SearchValue);

                            SELECT " + SelectList + @" FROM Employees 
                            WHERE (@SearchValue = N'') 
                               OR (FullName LIKE @SearchValue) 
                               OR (Phone LIKE @SearchValue) 
                               OR (Email LIKE @SearchValue)
                               OR (Address LIKE @SearchValue)
                            ORDER BY FullName
                            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                var parameters = new
                {
                    SearchValue = $"%{input.SearchValue}%",
                    input.Offset,
                    input.PageSize
                };

                using (var multi = await connection.QueryMultipleAsync(sql, parameters))
                {
                    var rowCount = await multi.ReadFirstAsync<int>();
                    var data = (await multi.ReadAsync<Employee>()).ToList();

                    return new PagedResult<Employee>
                    {
                        Page = input.Page,
                        PageSize = input.PageSize,
                        RowCount = rowCount,
                        DataItems = data
                    };
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using (var connection = GetConnection())
            {
                var sql = @"UPDATE Employees 
                            SET FullName = @FullName, 
                                BirthDate = @BirthDate, 
                                Address = @Address, 
                                Phone = @Phone, 
                                Email = @Email, 
                                Photo = @Photo, 
                               IsWorking = @IsWorking,
                                RoleNames = @RoleNames
                            WHERE EmployeeID = @EmployeeID";
                var parameters = new
                {
                    data.FullName,
                    data.BirthDate,
                    data.Address,
                    data.Phone,
                    data.Email,
                    data.Photo,
                    data.RoleNames,
                    data.IsWorking,
                    data.EmployeeID
                };
                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Kiểm tra email trùng
        /// </summary>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using (var connection = GetConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Employees WHERE Email = @Email AND EmployeeID <> @EmployeeID)
                                SELECT 1
                            ELSE
                                SELECT 0";
                var parameters = new { Email = email, EmployeeID = id };
                var result = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return result == 0;
            }
        }
        public async Task<bool> ChangePasswordAsync(int employeeID, string password)
        {
            using (var connection = GetConnection())
            {
                var sql = @"UPDATE Employees 
                    SET Password = @Password 
                    WHERE EmployeeID = @EmployeeID";
                var parameters = new { EmployeeID = employeeID, Password = password };
                int rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> UpdateRolesAsync(int employeeID, string[] roleNames)
        {
            using (var connection = GetConnection())
            {
                // 1. Xóa hết các quyền cũ của nhân viên này trong bảng EmployeeRoles
                var sqlDelete = "DELETE FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                await connection.ExecuteAsync(sqlDelete, new { EmployeeID = employeeID });

                // 2. Thêm lại các quyền mới từ mảng roleNames
                if (roleNames != null && roleNames.Length > 0)
                {
                    var sqlInsert = "INSERT INTO EmployeeRoles(EmployeeID, RoleName) VALUES(@EmployeeID, @RoleName)";
                    foreach (var role in roleNames)
                    {
                        await connection.ExecuteAsync(sqlInsert, new { EmployeeID = employeeID, RoleName = role });
                    }
                }
                return true;
            }
        }

        public async Task<IList<string>> GetRolesAsync(int employeeID)
        {
            using (var connection = GetConnection())
            {
                var sql = "SELECT RoleName FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                var result = await connection.QueryAsync<string>(sql, new { EmployeeID = employeeID });
                return result.ToList();
            }
        }
    }
}

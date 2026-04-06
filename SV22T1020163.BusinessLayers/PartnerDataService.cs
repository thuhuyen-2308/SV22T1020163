using SV22T1020163.DataLayers.Interfaces;
using SV22T1020163.DataLayers.SQLServer;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.Partner;

namespace SV22T1020163.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến các đối tác của hệ thống
    /// bao gồm: nhà cung cấp (Supplier), khách hàng (Customer) và người giao hàng (Shipper)
    /// </summary>
    public static class PartnerDataService
    {
        private static readonly IGenericRepository<Supplier> supplierDB;
        private static readonly ICustomerRepository customerDB;
        private static readonly IGenericRepository<Shipper> shipperDB;

        static PartnerDataService()
        {
            supplierDB = new SupplierRepository(Configuration.ConnectionString);
            customerDB = new CustomerRepository(Configuration.ConnectionString);
            shipperDB = new ShipperRepository(Configuration.ConnectionString);
        }

        #region Supplier

        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await supplierDB.ListAsync(input);
        }

        public static async Task<Supplier?> GetSupplierAsync(int supplierID)
        {
            return await supplierDB.GetAsync(supplierID);
        }

        /// <summary>
        /// Bổ sung nhà cung cấp mới
        /// </summary>
        public static async Task<int> AddSupplierAsync(Supplier data)
        {
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                throw new ArgumentException("Tên nhà cung cấp không được để trống");

            return await supplierDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        public static async Task<bool> UpdateSupplierAsync(Supplier data)
        {
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                return false;

            return await supplierDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteSupplierAsync(int supplierID)
        {
            if (await supplierDB.IsUsedAsync(supplierID))
                return false;

            return await supplierDB.DeleteAsync(supplierID);
        }

        public static async Task<bool> IsUsedSupplierAsync(int supplierID)
        {
            return await supplierDB.IsUsedAsync(supplierID);
        }

        #endregion

        #region Customer

        public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
        {
            return await customerDB.ListAsync(input);
        }

        public static async Task<Customer?> GetCustomerAsync(int customerID)
        {
            return await customerDB.GetAsync(customerID);
        }

        /// <summary>
        /// Bổ sung khách hàng mới
        /// </summary>
        public static async Task<int> AddCustomerAsync(Customer data)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName) || string.IsNullOrWhiteSpace(data.Email))
                throw new ArgumentException("Tên khách hàng và Email không được để trống");

            if (!string.IsNullOrEmpty(data.Password))
                data.Password = PasswordHasher.HashMd5(data.Password);

            return await customerDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        public static async Task<bool> UpdateCustomerAsync(Customer data)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                return false;

            return await customerDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            if (await customerDB.IsUsedAsync(customerID))
                return false;

            return await customerDB.DeleteAsync(customerID);
        }

        public static async Task<bool> IsUsedCustomerAsync(int customerID)
        {
            return await customerDB.IsUsedAsync(customerID);
        }

        public static async Task<bool> ValidatelCustomerEmailAsync(string email, int customerID = 0)
        {
            return await customerDB.ValidateEmailAsync(email, customerID);
        }

        public static async Task<bool> ChangePasswordCustomerAsync(int id, string password)
        {
            string hashed = string.IsNullOrEmpty(password) ? "" : PasswordHasher.HashMd5(password);
            return await customerDB.ChangePasswordAsync(id, hashed);
        }

        public static async Task<Customer?> AuthorizeCustomerAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || password == null)
                return null;

            string hash = PasswordHasher.HashMd5(password);
            var customer = await customerDB.AuthorizeAsync(email, hash);
            if (customer != null)
                return customer;

            // Kiểm tra trường hợp mật khẩu chưa băm (dữ liệu cũ)
            return await customerDB.AuthorizeAsync(email, password);
        }

        #endregion

        #region Shipper

        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
        {
            return await shipperDB.ListAsync(input);
        }

        public static async Task<Shipper?> GetShipperAsync(int shipperID)
        {
            return await shipperDB.GetAsync(shipperID);
        }

        /// <summary>
        /// Bổ sung người giao hàng mới
        /// </summary>
        public static async Task<int> AddShipperAsync(Shipper data)
        {
            if (string.IsNullOrWhiteSpace(data.ShipperName) || string.IsNullOrWhiteSpace(data.Phone))
                throw new ArgumentException("Tên người giao hàng và số điện thoại không được để trống");

            return await shipperDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        public static async Task<bool> UpdateShipperAsync(Shipper data)
        {
            if (string.IsNullOrWhiteSpace(data.ShipperName))
                return false;

            return await shipperDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            if (await shipperDB.IsUsedAsync(shipperID))
                return false;

            return await shipperDB.DeleteAsync(shipperID);
        }

        public static async Task<bool> IsUsedShipperAsync(int shipperID)
        {
            return await shipperDB.IsUsedAsync(shipperID);
        }

        #endregion
    }
}
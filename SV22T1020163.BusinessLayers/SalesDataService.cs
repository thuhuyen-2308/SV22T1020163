using SV22T1020163.DataLayers.Interfaces;
using SV22T1020163.DataLayers.SQLServer;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.Sales;

namespace SV22T1020163.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến bán hàng
    /// bao gồm: đơn hàng (Order) và chi tiết đơn hàng (OrderDetail).
    /// </summary>
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;

        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Order

        public static async Task<PagedResult<OrderViewInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            return await orderDB.ListAsync(input);
        }

        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            return await orderDB.GetAsync(orderID);
        }

        /// <summary>
        /// Tạo đơn hàng mới
        /// </summary>
        public static async Task<int> AddOrderAsync(Order data)
        {
            data.Status = OrderStatusEnum.New;
            data.OrderTime = DateTime.Now;

            return await orderDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng (Chỉ cho phép khi đơn hàng mới tạo)
        /// </summary>
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            // Chỉ cho phép cập nhật thông tin giao hàng khi đơn hàng đang ở trạng thái Chờ duyệt
            if (order.Status != OrderStatusEnum.New)
                return false;

            return await orderDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa đơn hàng (Chỉ cho phép xóa khi đơn hàng bị từ chối hoặc bị hủy)
        /// </summary>
        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.Rejected && order.Status != OrderStatusEnum.Cancelled)
                return false;

            return await orderDB.DeleteAsync(orderID);
        }

        #endregion

        #region Order Status Processing (Logic giữ nguyên vì bạn đã làm tốt)

        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = OrderStatusEnum.Accepted;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            order.EmployeeID = employeeID;
            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Rejected;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Cancelled;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Accepted) return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Shipping;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Shipping) return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Completed;

            return await orderDB.UpdateAsync(order);
        }

        #endregion

        #region Order Detail

        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            return await orderDB.ListDetailsAsync(orderID);
        }

        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return await orderDB.GetDetailAsync(orderID, productID);
        }

        /// <summary>
        /// Thêm mặt hàng vào đơn hàng (Chỉ cho phép khi đơn hàng đang chờ duyệt hoặc đã duyệt nhưng chưa giao)
        /// </summary>
        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.AddDetailAsync(data);
        }

        /// <summary>
        /// Cập nhật số lượng/giá bán của mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.UpdateDetailAsync(data);
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng
        /// </summary>
        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.DeleteDetailAsync(orderID, productID);
        }

        #endregion
    }
}
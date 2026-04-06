using SV22T1020163.Models.Common;

namespace SV22T1020163.Models.Sales
{
    /// <summary>
    /// Đầu vào tìm kiếm, phân trang đơn hàng
    /// </summary>
    public class OrderSearchInput : PaginationSearchInput
    {
        /// <summary>
        /// Khi có giá trị (Shop: lịch sử đơn của tôi), chỉ lấy đơn của khách này.
        /// Admin để null để tìm theo SearchValue (tỉnh/địa chỉ) như cũ.
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public OrderStatusEnum Status { get; set; }
        /// <summary>
        /// Từ ngày (ngày lập đơn hàng)
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// Đến ngày (ngày lập đơn hàng)
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
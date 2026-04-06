using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Partner;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize(Roles = AppRoles.AllStaff)]
    public class CustomerController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string CUSTOMER_SEARCH_CONDITION = "CustomerSearchCondition";

        /// <summary>
        /// Giao diện tìm kiếm và hiển thị danh sách khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<SV22T1020163.Models.Common.PaginationSearchInput>(CUSTOMER_SEARCH_CONDITION);
            if (input == null)
            {
                input = new SV22T1020163.Models.Common.PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }

        public async Task<IActionResult> Search(int page = 1, string searchValue = "")
        {
            var input = new SV22T1020163.Models.Common.PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue
            };
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_CONDITION, input);
            var data = await SV22T1020163.BusinessLayers.PartnerDataService.ListCustomersAsync(input);
            return PartialView(data);
        }

        /// <summary>
        /// Giao diện thêm mới khách hàng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            var data = new Customer()
            {
                CustomerID = 0,
                IsLocked = false
            };
            return View(data);
        }

        /// <summary>
        /// Xử lý thêm mới khách hàng
        /// </summary>
        /// <param name="customerName">Tên khách hàng cần thêm</param>
        /// <param name="contactName">Tên giao dịch cần thêm</param>
        /// <param name="phone">Số điện thoại cần thêm</param>
        /// <param name="email">Địa chỉ email cần thêm</param>
        /// <param name="address">Địa chỉ cần thêm</param>
        /// <param name="province">Tỉnh thành cần thêm</param>
        /// <param name="password">Mật khẩu đăng nhập cần thêm</param>
        /// <param name="isLocked">Trạng thái khóa cần thêm (true: khóa, false: không khóa)</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(Customer data)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên liên hệ không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh thành");
            
            //Kiểm tra email trùng
            if (!string.IsNullOrWhiteSpace(data.Email) && !await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID))
                ModelState.AddModelError(nameof(data.Email), "Email này đã được sử dụng bởi khách hàng khác");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Bổ sung khách hàng";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            // Xử lý thêm mới khách hàng
            int id = await PartnerDataService.AddCustomerAsync(data);
            if (id <= 0)
            {
                ModelState.AddModelError("", "Không thể bổ sung khách hàng. Vui lòng thử lại sau.");
                ViewBag.Title = "Bổ sung khách hàng";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Giao diện chỉnh sửa khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            var data = await PartnerDataService.GetCustomerAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần cập nhật</param>
        /// <param name="customerName">Tên khách hàng cần cập nhật</param>
        /// <param name="contactName">Tên giao dịch cần cập nhật</param>
        /// <param name="phone">Số điện thoại cần cập nhật</param>
        /// <param name="email">Địa chỉ email cần cập nhật</param>
        /// <param name="address">Địa chỉ cần cập nhật</param>
        /// <param name="province">Tỉnh thành cần cập nhật</param>
        /// <param name="isLocked">Trạng thái khóa cần cập nhật (true: khóa, false: không khóa)</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(Customer data)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên liên hệ không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh thành");

            //Kiểm tra email trùng
            if (!string.IsNullOrWhiteSpace(data.Email) && !await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID))
                ModelState.AddModelError(nameof(data.Email), "Email này đã được sử dụng bởi khách hàng khác");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Cập nhật thông tin khách hàng";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            // Xử lý cập nhật thông tin khách hàng
            bool result = await PartnerDataService.UpdateCustomerAsync(data);
            if (!result)
            {
                ModelState.AddModelError("", "Không thể cập nhật thông tin khách hàng. Vui lòng thử lại sau.");
                ViewBag.Title = "Cập nhật thông tin khách hàng";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Giao diện xác nhận xóa khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.Title = "Xóa khách hàng";
            var data = await PartnerDataService.GetCustomerAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        /// <summary>
        /// Xử lý xóa khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa</param>
        /// <param name="confirm">Xác nhận xóa</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            bool result = await PartnerDataService.DeleteCustomerAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng này. Có thể là do khách hàng đang có dữ liệu liên quan (ví dụ: các đơn hàng).";
                return RedirectToAction("Delete", new { id = id });
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Giao diện thay đổi mật khẩu khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            ViewBag.Title = "Đổi mật khẩu khách hàng";
            var data = await PartnerDataService.GetCustomerAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        /// <summary>
        /// Xử lý thay đổi mật khẩu khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần cập nhật mật khẩu</param>
        /// <param name="newPassword">Mật khẩu mới cần thiết lập</param>
        /// <param name="confirmPassword">Xác nhận mật khẩu mới cần thiết lập (phải trùng với newPassword)</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới");
            if (string.IsNullOrWhiteSpace(confirmPassword))
                ModelState.AddModelError("confirmPassword", "Vui lòng xác nhận lại mật khẩu");
            if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Đổi mật khẩu khách hàng";
                var data = await PartnerDataService.GetCustomerAsync(id);
                return View(data);
            }

            bool result = await PartnerDataService.ChangePasswordCustomerAsync(id, newPassword);
            if (!result)
            {
                ModelState.AddModelError("", "Không thể đổi mật khẩu. Vui lòng thử lại sau.");
                ViewBag.Title = "Đổi mật khẩu khách hàng";
                var data = await PartnerDataService.GetCustomerAsync(id);
                return View(data);
            }

            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.Partner;
using SV22T1020163.BusinessLayers;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize(Roles = AppRoles.AdminManager)]
    public class ShipperController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string SHIPPER_SEARCH_CONDITION = "ShipperSearchCondition";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị danh sách người giao hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH_CONDITION);
            if (input == null)
            {
                input = new PaginationSearchInput()
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
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue
            };
            ApplicationContext.SetSessionData(SHIPPER_SEARCH_CONDITION, input);
            var data = await PartnerDataService.ListShippersAsync(input);
            return PartialView(data);
        }

        /// <summary>
        /// Giao diện thêm mới người giao hàng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            var data = new Shipper { ShipperID = 0 };
            return View("Edit", data);
        }

        /// <summary>
        /// Xử lý thêm mới người giao hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Shipper data)
        {
            if (string.IsNullOrWhiteSpace(data.ShipperName))
                ModelState.AddModelError(nameof(data.ShipperName), "Tên người giao hàng không được để trống");

            if (!ModelState.IsValid)
                return View("Edit", data);

            await PartnerDataService.AddShipperAsync(data);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Giao diện chỉnh sửa người giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await PartnerDataService.GetShipperAsync(id);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin người giao hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(Shipper data)
        {
            if (string.IsNullOrWhiteSpace(data.ShipperName))
                ModelState.AddModelError(nameof(data.ShipperName), "Tên người giao hàng không được để trống");

            if (!ModelState.IsValid)
                return View("Edit", data);

            await PartnerDataService.UpdateShipperAsync(data);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Giao diện xác nhận xóa người giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await PartnerDataService.GetShipperAsync(id);
            if (data == null)
                return RedirectToAction(nameof(Index));

            ViewBag.IsUsed = await PartnerDataService.IsUsedShipperAsync(id);
            return View(data);
        }

        /// <summary>
        /// Xử lý xóa người giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa</param>
        /// <param name="confirm">Xác nhận xóa</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            if (string.IsNullOrWhiteSpace(confirm))
                return RedirectToAction(nameof(Index));

            var ok = await PartnerDataService.DeleteShipperAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa người giao hàng này. Có thể đang được sử dụng trong đơn hàng.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models.Common;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Partner;
using SV22T1020163.Models.DataDictionary;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize(Roles = AppRoles.AdminManager)]
    public class SupplierController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string SUPPLIER_SEARCH_CONDITION = "SupplierSearchCondition";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH_CONDITION);
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
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_CONDITION, input);
            var data = await PartnerDataService.ListSuppliersAsync(input);
            return PartialView(data);
        }

        /// <summary>
        /// Giao diện thêm mới nhà cung cấp
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            var data = new Supplier()
            {
                SupplierID = 0
            };
            return View("Edit", data);
        }

        /// <summary>
        /// Xử lý thêm mới nhà cung cấp
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Supplier data)
        {
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên liên hệ không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh thành");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Bổ sung nhà cung cấp";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View("Edit", data);
            }

            int id = await PartnerDataService.AddSupplierAsync(data);
            if (id <= 0)
            {
                ModelState.AddModelError("", "Không thể bổ sung nhà cung cấp. Vui lòng thử lại sau.");
                ViewBag.Title = "Bổ sung nhà cung cấp";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View("Edit", data);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Giao diện chỉnh sửa nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            var data = await PartnerDataService.GetSupplierAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin nhà cung cấp
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(Supplier data)
        {
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên liên hệ không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh thành");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            bool result = await PartnerDataService.UpdateSupplierAsync(data);
            if (!result)
            {
                ModelState.AddModelError("", "Không thể cập nhật thông tin nhà cung cấp. Vui lòng thử lại sau.");
                ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.Title = "Xóa nhà cung cấp";
            var data = await PartnerDataService.GetSupplierAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            ViewBag.IsUsed = await PartnerDataService.IsUsedSupplierAsync(id);

            return View(data);
        }

        /// <summary>
        /// Xử lý xóa nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <param name="confirm">Xác nhận xóa</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            bool result = await PartnerDataService.DeleteSupplierAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp này. Có thể là do nhà cung cấp đang có dữ liệu liên quan.";
                return RedirectToAction("Delete", new { id = id });
            }
            return RedirectToAction("Index");
        }
    }
}

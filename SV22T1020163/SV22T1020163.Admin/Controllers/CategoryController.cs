using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models.Catalog;
using SV22T1020163.Models.Common;
using SV22T1020163.BusinessLayers;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize(Roles = AppRoles.AdminManager)]
    public class CategoryController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string CATEGORY_SEARCH_CONDITION = "CategorySearchCondition";
        /// <summary>
        /// Giao diện tìm kiếm và hiển thị danh sách loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH_CONDITION);
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
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_CONDITION, input);
            var data = await CatalogDataService.ListCategoriesAsync(input);
            return PartialView(data);
        }

        /// <summary>
        /// Giao diện thêm mới loại hàng
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var data = new Category { CategoryID = 0 };
            return View("Edit", data);
        }

        /// <summary>
        /// Xử lý thêm mới loại hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Category data)
        {
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(Category.CategoryName), "Tên loại hàng là bắt buộc.");

            if (!ModelState.IsValid)
                return View("Edit", data);

            await CatalogDataService.AddCategoryAsync(data);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Giao diện chỉnh sửa loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin loại hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(Category data)
        {
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(Category.CategoryName), "Tên loại hàng là bắt buộc.");

            if (!ModelState.IsValid)
                return View(data);

            await CatalogDataService.UpdateCategoryAsync(data);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Giao diện xác nhận xóa loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction(nameof(Index));

            ViewBag.IsUsed = await CatalogDataService.IsUsedCategoryAsync(id);
            return View(data);
        }

        /// <summary>
        /// Xử lý xóa loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <param name="confirm">Xác nhận xóa</param>
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            if (!string.IsNullOrEmpty(confirm))
                await CatalogDataService.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

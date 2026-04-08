using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.Catalog;
using SV22T1020163.BusinessLayers;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 12;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ProductController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        private async Task<string?> SaveUploadedPhotoAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;
            string folder = Path.Combine(MediaPaths.ResolveRoot(_env, _configuration), subfolder);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
            return fileName;
        }

        [Authorize(Roles = AppRoles.AllStaff)]
        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION) ?? new ProductSearchInput
            {
                Page = 1,
                PageSize = PAGE_SIZE,
                SearchValue = "",
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };
            ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            return View(input);
        }

        [Authorize(Roles = AppRoles.AllStaff)]
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, input);
            var data = await CatalogDataService.ListProductsAsync(input);
            return PartialView(data);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            var product = new Product { ProductID = 0, IsSelling = true, Photo = "nophoto.png" };
            return View("Edit", product);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> Create(Product data, IFormFile? uploadPhoto)
        {
            // Kiểm tra tính hợp lệ của dữ liệu (Tránh lỗi ArgumentException như trong ảnh 1)
            if (string.IsNullOrWhiteSpace(data.ProductName)) ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");
            if (data.CategoryID <= 0) ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
            if (data.SupplierID <= 0) ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");

            if (!ModelState.IsValid)
            {
                // Sửa lỗi FETCH clause bằng cách không để PageSize = -1 (Ảnh 2)
                ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
                ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
                return View("Edit", data);
            }

            if (uploadPhoto != null) data.Photo = await SaveUploadedPhotoAsync(uploadPhoto, "products");
            int productId = await CatalogDataService.AddProductAsync(data);
            return RedirectToAction("Edit", new { id = productId });
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null) return RedirectToAction("Index");

            ViewBag.ProductID = id;
            ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> Edit(Product data, IFormFile? uploadPhoto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
                ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
                return View(data);
            }

            if (uploadPhoto != null) data.Photo = await SaveUploadedPhotoAsync(uploadPhoto, "products");
            await CatalogDataService.UpdateProductAsync(data);
            return RedirectToAction("Index");
        }

        // --- QUẢN LÝ ẢNH (PHOTOS) ---
        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public IActionResult CreatePhoto(int id)
        {
            var photo = new ProductPhoto { ProductID = id, DisplayOrder = 1 };
            return View("EditPhoto", photo);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> CreatePhoto(ProductPhoto data, IFormFile? PhotoFile)
        {
            // Tránh lỗi FOREIGN KEY (Ảnh 3, 5) và lỗi NULL Description (Ảnh 4)
            if (string.IsNullOrWhiteSpace(data.Description)) ModelState.AddModelError(nameof(data.Description), "Vui lòng nhập mô tả ảnh");
            if (PhotoFile == null) ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn ảnh");

            if (!ModelState.IsValid) return View("EditPhoto", data);

            data.Photo = await SaveUploadedPhotoAsync(PhotoFile, "products") ?? "";
            await CatalogDataService.AddPhotoAsync(data);
            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> EditPhoto(int id, long photoId)
        {
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (photo == null) return RedirectToAction("Edit", new { id });
            return View(photo);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> EditPhoto(ProductPhoto data, IFormFile? PhotoFile)
        {
            if (string.IsNullOrWhiteSpace(data.Description)) ModelState.AddModelError(nameof(data.Description), "Vui lòng nhập mô tả ảnh");
            if (!ModelState.IsValid) return View(data);

            if (PhotoFile != null) data.Photo = await SaveUploadedPhotoAsync(PhotoFile, "products") ?? data.Photo;
            await CatalogDataService.UpdatePhotoAsync(data);
            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        [HttpPost] // Lưu ý dùng Post cho xóa để bảo mật
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> DeletePhoto(int id, long photoId)
        {
            await CatalogDataService.DeletePhotoAsync(photoId);
            return RedirectToAction("Edit", new { id });
        }

        // --- QUẢN LÝ THUỘC TÍNH (ATTRIBUTES) ---
        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public IActionResult CreateAttribute(int id)
        {
            var attr = new ProductAttribute { ProductID = id, DisplayOrder = 1 };
            return View("EditAttribute", attr);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> CreateAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName)) ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");
            if (string.IsNullOrWhiteSpace(data.AttributeValue)) ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị không được để trống");
            if (!ModelState.IsValid) return View("EditAttribute", data);

            await CatalogDataService.AddAttributeAsync(data);
            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> EditAttribute(int id, long attributeId)
        {
            var attr = await CatalogDataService.GetAttributeAsync(attributeId);
            if (attr == null) return RedirectToAction("Edit", new { id });
            return View(attr);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> EditAttribute(ProductAttribute data)
        {
            if (!ModelState.IsValid) return View(data);
            await CatalogDataService.UpdateAttributeAsync(data);
            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        // Đảm bảo dùng [HttpPost] vì Form của bạn gửi Post
        [HttpPost]
        [Authorize(Roles = AppRoles.AdminManager)]
        
        public async Task<IActionResult> DeleteAttribute(int id, long attributeId, string confirm = "")
        {
            if (confirm == "yes")
            {
                await CatalogDataService.DeleteAttributeAsync(attributeId);
            }
            return RedirectToAction("Edit", new { id = id });
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManager)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null) return RedirectToAction("Index");
            return View(product);
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm = "")
        {
            if (confirm == "yes")
            {
                bool result = await CatalogDataService.DeleteProductAsync(id);
                if (!result)
                {
                    ViewBag.IsUsed = true; // Báo cho View biết là mặt hàng đang được sử dụng, không xóa được
                    var data = await CatalogDataService.GetProductAsync(id);
                    return View(data);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
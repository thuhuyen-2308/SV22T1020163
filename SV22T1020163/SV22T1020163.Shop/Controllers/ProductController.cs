using Microsoft.AspNetCore.Mvc;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Catalog;
using SV22T1020163.Models.Common;

namespace SV22T1020163.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 12;

        public async Task<IActionResult> Index(string? search, int categoryId = 0, decimal minPrice = 0, decimal maxPrice = 0, int page = 1)
        {
            var input = new ProductSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = search ?? "",
                CategoryID = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var data = await CatalogDataService.ListProductsAsync(input);
            ViewBag.SearchInput = input;

            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput { Page = 1, PageSize = 200 });
            ViewBag.Categories = categories.DataItems;

            return View(data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);

            var related = await CatalogDataService.ListProductsAsync(new ProductSearchInput
            {
                Page = 1,
                PageSize = 4,
                CategoryID = product.CategoryID ?? 0
            });
            ViewBag.RelatedProducts = related.DataItems.Where(p => p.ProductID != id).Take(4).ToList();

            return View(product);
        }
    }
}

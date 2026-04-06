using Microsoft.AspNetCore.Mvc;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Catalog;
using SV22T1020163.Models.Common;

namespace SV22T1020163.Shop.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var products = await CatalogDataService.ListFeaturedProductsByCategoryAsync(8);
            ViewBag.FeaturedProducts = products;

            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 20,
                SearchValue = ""
            });
            ViewBag.Categories = categories.DataItems;

            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

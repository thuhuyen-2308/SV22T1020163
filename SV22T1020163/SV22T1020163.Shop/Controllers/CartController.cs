using Microsoft.AspNetCore.Mvc;
using SV22T1020163.BusinessLayers;

namespace SV22T1020163.Shop.Controllers
{
    public class CartController : Controller
    {
        private List<CartItem> GetCart()
        {
            return ApplicationContext.GetSessionData<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            ApplicationContext.SetSessionData("Cart", cart);
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                var product = await CatalogDataService.GetProductAsync(productId);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo ?? "",
                        Unit = product.Unit,
                        Price = product.Price,
                        Quantity = quantity
                    });
                }
            }
            SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { count = cart.Sum(c => c.Quantity) });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity > 999) quantity = 999;
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductID == productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("Index");
        }


    }
}

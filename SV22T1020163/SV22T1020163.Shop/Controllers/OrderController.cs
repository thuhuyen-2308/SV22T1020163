using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Sales;
using System.Security.Claims;

namespace SV22T1020163.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private int GetCustomerId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }

            var customer = await PartnerDataService.GetCustomerAsync(GetCustomerId());
            ViewBag.Customer = customer;
            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(c => c.TotalPrice);
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string deliveryProvince, string deliveryAddress, string deliveryPhone)
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Kiểm tra thêm deliveryPhone không được để trống
            if (string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress) || string.IsNullOrWhiteSpace(deliveryPhone))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin giao hàng và số điện thoại.";
                return RedirectToAction("Checkout");
            }

            int customerId = GetCustomerId();

            var order = new Order
            {
                CustomerID = customerId,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                DeliveryPhone = deliveryPhone 
            };

            int orderId = await SalesDataService.AddOrderAsync(order);

            if (orderId > 0)
            {
                foreach (var item in cart)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail
                    {
                        OrderID = orderId,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        SalePrice = item.Price
                    });
                }

                ApplicationContext.SetSessionData("Cart", new List<CartItem>());
                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("Detail", new { id = orderId });
            }

            TempData["Error"] = "Đã có lỗi xảy ra, vui lòng thử lại.";
            return RedirectToAction("Checkout");
        }

        public async Task<IActionResult> History(int status = 0, int page = 1)
        {
            int customerId = GetCustomerId();
            var input = new OrderSearchInput
            {
                Page = page,
                PageSize = 10,
                SearchValue = "",
                CustomerId = customerId,
                Status = (OrderStatusEnum)status
            };

            var data = await SalesDataService.ListOrdersAsync(input);
            ViewBag.CurrentStatus = status;
            return View(data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != GetCustomerId())
                return RedirectToAction("History");

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Details = details;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != GetCustomerId())
                return RedirectToAction("History");

            bool result = await SalesDataService.CancelOrderAsync(id);
            TempData[result ? "Success" : "Error"] = result
                ? "Đã hủy đơn hàng thành công."
                : "Không thể hủy đơn hàng này.";

            return RedirectToAction("Detail", new { id });
        }
    }
}
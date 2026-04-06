using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.Catalog;
using SV22T1020163.Models.Sales;
using SV22T1020163.BusinessLayers;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize(Roles = AppRoles.AllStaff)]
    public class OrderController : Controller
    {
        private const int PAGE_SIZE = 20;
        private const string ORDER_SEARCH_CONDITION = "OrderSearchCondition";
        private const int PRODUCT_PAGE_SIZE = 5;

        private int GetCurrentEmployeeID()
        {
            var userData = ApplicationContext.GetSessionData<Models.Security.UserAccount>("UserData");
            if (userData != null && int.TryParse(userData.UserID, out int id))
                return id;
            return 1;
        }

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_CONDITION);
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    Status = 0
                };
            }
            return View(input);
        }

        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            ApplicationContext.SetSessionData(ORDER_SEARCH_CONDITION, input);
            var data = await SalesDataService.ListOrdersAsync(input);
            return PartialView(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await PartnerDataService.ListCustomersAsync(new PaginationSearchInput { Page = 1, PageSize = 10000 });
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();

            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>("Cart") ?? new List<OrderDetail>();
            var cartDisplay = new List<dynamic>();
            decimal total = 0;
            foreach (var item in cart)
            {
                var product = await CatalogDataService.GetProductAsync(item.ProductID);
                cartDisplay.Add(new
                {
                    ProductID = item.ProductID,
                    ProductName = product?.ProductName ?? $"Sản phẩm #{item.ProductID}",
                    Unit = product?.Unit ?? "",
                    Photo = product?.Photo ?? "",
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice,
                    TotalPrice = item.TotalPrice
                });
                total += item.TotalPrice;
            }
            ViewBag.CartItems = cartDisplay;
            ViewBag.CartTotal = total;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int customerID, string? deliveryProvince, string? deliveryAddress)
        {
            void PreserveFormData()
            {
                TempData["SelectedCustomerID"] = customerID.ToString();
                TempData["SelectedProvince"] = deliveryProvince ?? "";
                TempData["SelectedAddress"] = deliveryAddress ?? "";
            }

            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>("Cart");
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng chọn mặt hàng cần bán.";
                PreserveFormData();
                return RedirectToAction("Create");
            }

            if (customerID <= 0 || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin khách hàng và nơi giao hàng.";
                PreserveFormData();
                return RedirectToAction("Create");
            }

            var order = new Order
            {
                CustomerID = customerID,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                EmployeeID = GetCurrentEmployeeID(),
                OrderTime = DateTime.Now,
                Status = OrderStatusEnum.New
            };

            int orderID = await SalesDataService.AddOrderAsync(order);
            if (orderID > 0)
            {
                foreach (var item in cart)
                {
                    item.OrderID = orderID;
                    await SalesDataService.AddDetailAsync(item);
                }
                ApplicationContext.SetSessionData("Cart", new List<OrderDetail>());
            }

            return RedirectToAction("Detail", new { id = orderID });
        }

        [HttpPost]
        public IActionResult AddToCart(int productID, int quantity, decimal salePrice)
        {
            if (quantity <= 0) quantity = 1;

            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>("Cart") ?? new List<OrderDetail>();

            var existingItem = cart.FirstOrDefault(x => x.ProductID == productID);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.SalePrice = salePrice;
            }
            else
            {
                cart.Add(new OrderDetail
                {
                    ProductID = productID,
                    Quantity = quantity,
                    SalePrice = salePrice
                });
            }

            ApplicationContext.SetSessionData("Cart", cart);
            return RedirectToAction("Create");
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int productID, int quantity, decimal salePrice)
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>("Cart") ?? new List<OrderDetail>();
            var item = cart.FirstOrDefault(x => x.ProductID == productID);
            if (item != null)
            {
                item.Quantity = quantity > 0 ? quantity : 1;
                item.SalePrice = salePrice;
            }
            ApplicationContext.SetSessionData("Cart", cart);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productID)
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>("Cart") ?? new List<OrderDetail>();
            cart.RemoveAll(x => x.ProductID == productID);
            ApplicationContext.SetSessionData("Cart", cart);
            return RedirectToAction("Create");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            ApplicationContext.SetSessionData("Cart", new List<OrderDetail>());
            return RedirectToAction("Create");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            ViewBag.Details = await SalesDataService.ListDetailsAsync(id);
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> EditDetail(int id, int productId)
        {
            var detail = await SalesDataService.GetDetailAsync(id, productId);
            if (detail == null)
                return RedirectToAction("Detail", new { id });
            ViewBag.OrderID = id;
            return View(detail);
        }

        [HttpPost]
        public async Task<IActionResult> EditDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            var data = new OrderDetail
            {
                OrderID = orderID,
                ProductID = productID,
                Quantity = quantity > 0 ? quantity : 1,
                SalePrice = salePrice
            };
            await SalesDataService.UpdateDetailAsync(data);
            return RedirectToAction("Detail", new { id = orderID });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDetail(int id, int productId)
        {
            await SalesDataService.DeleteDetailAsync(id, productId);
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Accept(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int id, string confirm)
        {
            await SalesDataService.AcceptOrderAsync(id, GetCurrentEmployeeID());
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");

            ViewBag.Shippers = await PartnerDataService.ListShippersAsync(new PaginationSearchInput { Page = 1, PageSize = 10000 });
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            await SalesDataService.ShipOrderAsync(id, shipperID);
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Finish(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id, string confirm)
        {
            await SalesDataService.CompleteOrderAsync(id);
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string confirm)
        {
            await SalesDataService.CancelOrderAsync(id);
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string confirm)
        {
            await SalesDataService.RejectOrderAsync(id, GetCurrentEmployeeID());
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order != null && (order.Status == OrderStatusEnum.New
                || order.Status == OrderStatusEnum.Cancelled
                || order.Status == OrderStatusEnum.Rejected))
            {
                await SalesDataService.DeleteOrderAsync(id);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Detail", new { id });
        }

        public async Task<IActionResult> SearchProducts(string searchValue = "", int page = 1)
        {
            var input = new ProductSearchInput
            {
                Page = page,
                PageSize = PRODUCT_PAGE_SIZE,
                SearchValue = searchValue ?? "",
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };
            var data = await CatalogDataService.ListProductsAsync(input);
            return PartialView(data);
        }
    }
}

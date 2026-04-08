using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Partner;
using System.Security.Claims;

namespace SV22T1020163.Shop.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                return View();
            }

            var customer = await PartnerDataService.AuthorizeCustomerAsync(email, password);
            if (customer == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                new Claim(ClaimTypes.Name, customer.CustomerName),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim("ContactName", customer.ContactName ?? ""),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Customer data, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError("CustomerName", "Vui lòng nhập tên.");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(data.Password) || data.Password.Length < 6)
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự.");
            if (data.Password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp.");

            if (!string.IsNullOrWhiteSpace(data.Email))
            {
                bool emailOk = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, 0);
                if (!emailOk)
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (!ModelState.IsValid)
                return View(data);

            data.IsLocked = false;
            data.ContactName = data.ContactName ?? data.CustomerName;
            int id = await PartnerDataService.AddCustomerAsync(data);
            if (id > 0)
            {
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Đã có lỗi xảy ra, vui lòng thử lại.";
            return View(data);
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction("Logout");

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(customer);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(Customer data)
        {
            int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            data.CustomerID = customerId;

            var existing = await PartnerDataService.GetCustomerAsync(customerId);
            if (existing == null)
                return RedirectToAction("Logout");

            data.IsLocked = existing.IsLocked;
            data.Password = existing.Password;

            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError("CustomerName", "Vui lòng nhập tên.");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");

            if (!string.IsNullOrWhiteSpace(data.Email) && data.Email != existing.Email)
            {
                bool emailOk = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, customerId);
                if (!emailOk)
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            await PartnerDataService.UpdateCustomerAsync(data);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, data.CustomerID.ToString()),
                new Claim(ClaimTypes.Name, data.CustomerName),
                new Claim(ClaimTypes.Email, data.Email),
                new Claim("ContactName", data.ContactName ?? ""),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Kiểm tra và dùng ModelState để hiện chữ đỏ dưới từng ô
            if (string.IsNullOrWhiteSpace(oldPassword))
                ModelState.AddModelError("oldPassword", "Vui lòng nhập mật khẩu cũ.");
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                ModelState.AddModelError("newPassword", "Mật khẩu mới phải từ 6 ký tự.");
            if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp.");

            if (!ModelState.IsValid)
                return View();

            int customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            string email = User.FindFirstValue(ClaimTypes.Email) ?? "";

            var check = await PartnerDataService.AuthorizeCustomerAsync(email, oldPassword);
            if (check == null)
            {
                // Lỗi này không thuộc ô nào cụ thể thì dùng ViewBag.Error là đúng
                ViewBag.Error = "Mật khẩu cũ không chính xác.";
                return View();
            }

            await PartnerDataService.ChangePasswordCustomerAsync(customerId, newPassword);
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

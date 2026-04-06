using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.BusinessLayers;
using SV22T1020163.Models.Security;
using System.Security.Claims;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập email và mật khẩu.");
                return View();
            }

            var user = await SecurityDataService.AuthorizeAsync(email, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Đăng nhập không thành công. Vui lòng kiểm tra lại email và mật khẩu.");
                return View();
            }

            ApplicationContext.SetSessionData("UserData", user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Photo", user.Photo ?? ""),
            };

            if (!string.IsNullOrWhiteSpace(user.RoleNames))
            {
                foreach (var role in user.RoleNames.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    claims.Add(new Claim(ClaimTypes.Role, role.Trim().ToLowerInvariant()));
            }
            else
                claims.Add(new Claim(ClaimTypes.Role, AppRoles.Sale));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
                ModelState.AddModelError("oldPassword", "Vui lòng nhập mật khẩu cũ.");
            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới.");
            if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không trùng khớp.");

            if (!ModelState.IsValid)
                return View();

            var email = User.FindFirstValue(ClaimTypes.Email) ?? "";
            var checkUser = await SecurityDataService.AuthorizeAsync(email, oldPassword);
            if (checkUser == null)
            {
                ModelState.AddModelError("oldPassword", "Mật khẩu cũ không đúng.");
                return View();
            }

            bool result = await SecurityDataService.ChangePasswordAsync(email, newPassword);
            if (!result)
            {
                ModelState.AddModelError("", "Đổi mật khẩu thất bại. Vui lòng thử lại.");
                return View();
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

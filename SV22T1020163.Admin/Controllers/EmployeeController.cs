using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models.Common;
using SV22T1020163.Models.HR;
using SV22T1020163.Models.Security;
using SV22T1020163.BusinessLayers;
using System.Security.Claims;

namespace SV22T1020163.Admin.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class EmployeeController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string EMPLOYEE_SEARCH_CONDITION = "EmployeeSearchCondition";

        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public EmployeeController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        private async Task<string?> SaveUploadedPhotoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string folder = Path.Combine(MediaPaths.ResolveRoot(_env, _configuration), "employees");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH_CONDITION);
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
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_CONDITION, input);
            var data = await HRDataService.ListEmployeesAsync(input);
            return PartialView(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var employee = new Employee { IsWorking = true };
            return View("Edit", employee);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee data, IFormFile? uploadPhoto)
        {
            if (uploadPhoto != null)
            {
                string? fileName = await SaveUploadedPhotoAsync(uploadPhoto);
                if (fileName != null)
                    data.Photo = fileName;
            }

            await HRDataService.AddEmployeeAsync(data);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return RedirectToAction("Index");
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Employee data, IFormFile? uploadPhoto)
        {
            if (uploadPhoto != null)
            {
                string? fileName = await SaveUploadedPhotoAsync(uploadPhoto);
                if (fileName != null)
                    data.Photo = fileName;
            }
            else
            {
                var existing = await HRDataService.GetEmployeeAsync(data.EmployeeID);
                if (existing != null)
                    data.Photo = existing.Photo;
            }

            await HRDataService.UpdateEmployeeAsync(data);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == data.EmployeeID.ToString())
            {
                var fresh = await HRDataService.GetEmployeeAsync(data.EmployeeID);
                if (fresh != null)
                {
                    var roleText = string.IsNullOrWhiteSpace(fresh.RoleNames) ? AppRoles.Sale : fresh.RoleNames.Trim().ToLowerInvariant();
                    ApplicationContext.SetSessionData("UserData", new UserAccount
                    {
                        UserID = fresh.EmployeeID.ToString(),
                        UserName = fresh.Email,
                        Email = fresh.Email,
                        FullName = fresh.FullName,
                        Photo = fresh.Photo,
                        RoleNames = roleText
                    });

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, fresh.EmployeeID.ToString()),
                        new Claim(ClaimTypes.Name, fresh.FullName ?? ""),
                        new Claim(ClaimTypes.Email, fresh.Email ?? ""),
                        new Claim("Photo", fresh.Photo ?? ""),
                    };
                    foreach (var role in roleText.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        claims.Add(new Claim(ClaimTypes.Role, role.Trim().ToLowerInvariant()));

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return RedirectToAction("Index");
            ViewBag.IsUsed = await HRDataService.IsUsedEmployeeAsync(id);
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            if (!string.IsNullOrEmpty(confirm))
                await HRDataService.DeleteEmployeeAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(int id, string newPassword, string confirmPassword)
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangeRole(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangeRole(int id, string[] roles)
        {
            return RedirectToAction("Index");
        }
    }
}

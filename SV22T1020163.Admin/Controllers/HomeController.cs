using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020163.Admin;
using SV22T1020163.Models;

namespace SV22T1020163.Admin.Controllers;

[Authorize(Roles = AppRoles.AllStaff)]
public class HomeController : Controller
{
    /// <summary>
    /// Giao diện trang chủ của hệ thống
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

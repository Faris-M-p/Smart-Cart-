using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

/// <summary>Employees, roles, company/stores and settings.</summary>
public class AdminController : Controller
{
    public IActionResult Employees() => View();

    public IActionResult Roles() => View();

    public IActionResult Stores() => View();

    public IActionResult Settings() => View();
}

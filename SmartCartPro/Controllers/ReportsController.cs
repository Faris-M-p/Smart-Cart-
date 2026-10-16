using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

public class ReportsController : Controller
{
    public IActionResult Sales() => View();

    public IActionResult Products() => View();

    public IActionResult Customers() => View();
}

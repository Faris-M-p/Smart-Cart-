using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

/// <summary>Suppliers, purchases and store stock.</summary>
public class InventoryController : Controller
{
    public IActionResult Suppliers() => View();

    public IActionResult Purchases() => View();

    public IActionResult Stock() => View();
}

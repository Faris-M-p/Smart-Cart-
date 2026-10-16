using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

/// <summary>POS, invoices, returns and online orders.</summary>
public class SalesController : Controller
{
    public IActionResult Pos() => View();

    public IActionResult Invoices() => View();

    public IActionResult Returns() => View();

    public IActionResult OnlineOrders() => View();
}

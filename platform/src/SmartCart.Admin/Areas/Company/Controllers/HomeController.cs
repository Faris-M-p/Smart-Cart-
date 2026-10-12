using Microsoft.AspNetCore.Mvc;

namespace SmartCart.Admin.Areas.Company.Controllers;

[Area("Company")]
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Company Admin Dashboard";
        ViewData["Menus"] = new[]
        {
            "Dashboard", "Product Management", "Inventory and Stock", "Purchases and Suppliers",
            "POS and Billing", "Sales and Orders", "Customers", "Homepage Management",
            "Customer Theme Information", "Employees", "Roles and Permissions",
            "Attendance and Working Hours", "Leave Management", "Tasks and Assignments",
            "Reports and Analytics", "Finance and Accounts", "Notifications",
            "Company Settings", "Subscription and License Information", "Profile and Security"
        };
        return View();
    }
}

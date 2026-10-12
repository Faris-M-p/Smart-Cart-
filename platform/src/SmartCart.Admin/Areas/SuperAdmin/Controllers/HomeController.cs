using Microsoft.AspNetCore.Mvc;

namespace SmartCart.Admin.Areas.SuperAdmin.Controllers;

[Area("SuperAdmin")]
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Super Admin Dashboard";
        ViewData["Menus"] = new[]
        {
            "Dashboard", "Companies / Clients", "Company Registration", "Client Database Provisioning",
            "Subscription Plans", "Subscription and License Management", "Store Mode and Store Limits",
            "Module Management", "Theme Management", "Homepage Section Availability",
            "Client Activation / Deactivation", "Subscription Renewal", "Platform Monitoring",
            "Audit Logs", "Support and Announcements", "Super Admin Users and Permissions", "Platform Settings"
        };
        return View();
    }
}

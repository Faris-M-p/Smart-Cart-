using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

/// <summary>Employees, roles, company/stores and settings.</summary>
public class AdminController : Controller
{
    /// <summary>
    /// Admin landing page for employee list and management UI.
    /// Navigated from the admin menu to manage store staff.
    /// </summary>
    public IActionResult Employees() => View();

    /// <summary>
    /// Admin page for roles and permission assignment.
    /// Opened from admin navigation to configure access control.
    /// </summary>
    public IActionResult Roles() => View();

    /// <summary>
    /// Admin page for company and store configuration.
    /// Used to open settings screens linked from the admin area.
    /// </summary>
    public IActionResult Settings() => View();
}

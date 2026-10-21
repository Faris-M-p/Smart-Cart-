using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCartPro.Configuration;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

public class HomeController : Controller
{
    private readonly EnvironmentSettings _environmentSettings;
    private readonly IDashboardService _dashboard;
    private readonly ICurrentUserAccess _access;

    public HomeController(
        IOptions<EnvironmentSettings> environmentSettings,
        IDashboardService dashboard,
        ICurrentUserAccess access)
    {
        _environmentSettings = environmentSettings.Value;
        _dashboard = dashboard;
        _access = access;
    }

    /// <summary>
    /// Signed-in home: store KPIs when a store is selected, a Super Admin setup page, or a welcome page.
    /// Every authenticated user can open this; dashboard.view is not required.
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var input = new InputDashboardSelect
        {
            ActorUserId = AuthClaims.GetUserId(User) ?? 0,
            ActorFkStore = AuthClaims.GetFkStore(User) ?? 0,
            IsPlatformSuperAdmin = AuthClaims.IsPlatformSuperAdminUser(User),
            StoreName = AuthClaims.GetStoreName(User),
            HasDashboardView = await _access.CanAsync("dashboard.view", ct),
            CanPosSell = await _access.CanAsync("pos.sell", ct),
            CanProductsView = await _access.CanAsync("products.view", ct),
            CanPurchasesManage = await _access.CanAsync("purchases.manage", ct),
            CanInventoryView = await _access.CanAsync("inventory.view", ct),
            CanCustomersView = await _access.CanAsync("customers.view", ct),
            CanReportsView = await _access.CanAsync("reports.view", ct),
            CanSettingsManage = await _access.CanAsync("settings.manage", ct),
            CanUsersManage = await _access.CanAsync("users.manage", ct),
            ModulePos = await _access.ModuleEnabledAsync("POS", ct),
            ModuleProducts = await _access.ModuleEnabledAsync("PRODUCTS", ct),
            ModulePurchases = await _access.ModuleEnabledAsync("PURCHASES", ct),
            ModuleInventory = await _access.ModuleEnabledAsync("INVENTORY", ct),
            ModuleCustomers = await _access.ModuleEnabledAsync("CUSTOMERS", ct),
            ModuleReports = await _access.ModuleEnabledAsync("REPORTS", ct),
            ModuleSettings = await _access.ModuleEnabledAsync("SETTINGS", ct),
            ModuleUsers = await _access.ModuleEnabledAsync("USERS", ct)
        };
        var model = await _dashboard.GetHomeAsync(input, ct);
        return View(model);
    }

    /// <summary>
    /// Renders a user-safe error page for unhandled exceptions.
    /// Invoked by the exception handler middleware when a request fails.
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        if (exception is null)
        {
            return View(new ErrorViewModel
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Title = "Something went wrong",
                UserMessage = "We hit a snag while loading this page. Please try again.",
                RequestId = requestId
            });
        }

        var mapped = ExceptionErrorMapper.Map(exception);
        Response.StatusCode = mapped.HttpStatusCode;

        // Full developer text for browser console.log only when not Prod (never shown on the page UI).
        var developerMessage = _environmentSettings.IsProduction
            ? null
            : ExceptionErrorMapper.BuildFullDeveloperMessage(mapped.DeveloperMessage, exception);

        return View(new ErrorViewModel
        {
            StatusCode = mapped.StatusCode,
            Title = TitleForStatus(mapped.StatusCode),
            UserMessage = mapped.UserMessage,
            DeveloperMessage = developerMessage,
            RequestId = requestId
        });
    }

    /// <summary>
    /// Renders status-specific error views for non-success HTTP codes.
    /// Used by status-code re-execution (404 vs generic error layout).
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult StatusCodePage(int code)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var status = code > 0 ? code : StatusCodes.Status404NotFound;
        Response.StatusCode = status;

        if (status == StatusCodes.Status404NotFound)
        {
            return View("NotFound", new ErrorViewModel
            {
                StatusCode = 404,
                Title = "Page not found",
                UserMessage = "This aisle is empty — the page you asked for is not here.",
                RequestId = requestId
            });
        }

        return View("Error", new ErrorViewModel
        {
            StatusCode = status,
            Title = TitleForStatus(status),
            UserMessage = "Something went wrong while processing your request.",
            RequestId = requestId
        });
    }

    /// <summary>
    /// Friendly page shown when the installation license blocks a page (suspended, expired or missing).
    /// Reached by redirect from the license gate; AJAX callers receive JSON instead.
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult LicenseBlocked(string? reason = null)
    {
        var message = reason switch
        {
            "suspended" => "This installation's license is suspended. Please contact your administrator.",
            "expired" => "This installation's license has expired, so data entry is disabled. Please contact your administrator.",
            "none" => "No active license was found for this installation. Please contact your administrator.",
            _ => "The license for this installation does not allow this action. Please contact your administrator."
        };

        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("Error", new ErrorViewModel
        {
            StatusCode = StatusCodes.Status403Forbidden,
            Title = "License restricted",
            UserMessage = message,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /// <summary>
    /// Dedicated 404 page when a route or link targets a missing resource.
    /// Called directly or from routing when a not-found view is needed.
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NotFound", new ErrorViewModel
        {
            StatusCode = 404,
            Title = "Page not found",
            UserMessage = "This aisle is empty — the page you asked for is not here.",
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private static string TitleForStatus(int statusCode) =>
        statusCode switch
        {
            404 => "Page not found",
            401 or 440 or 498 => "Sign-in required",
            403 => "Access denied",
            408 => "Request timed out",
            _ => "Something went wrong"
        };
}

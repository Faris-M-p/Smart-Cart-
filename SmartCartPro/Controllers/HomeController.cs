using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCartPro.Configuration;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Controllers;

public class HomeController : Controller
{
    private readonly EnvironmentSettings _environmentSettings;

    public HomeController(IOptions<EnvironmentSettings> environmentSettings)
    {
        _environmentSettings = environmentSettings.Value;
    }

    public IActionResult Index() => View();

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

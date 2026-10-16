using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

/// <summary>Authentication screens (login, register, password recovery).</summary>
public class AuthController : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Register() => View();

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpGet]
    public IActionResult ResetPassword() => View();

    [HttpGet]
    public IActionResult Logout() => RedirectToAction(nameof(Login));
}

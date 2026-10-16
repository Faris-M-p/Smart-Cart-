using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Shared login for store admins and platform Super Admin.
/// Super Admin password success → email OTP screen; store admin → JWT cookie.
/// </summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginInput());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput input, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var result = await _authService.LoginAsync(input, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Sign-in failed.");
            return View(input);
        }

        if (result.RequiresOtp && !string.IsNullOrWhiteSpace(result.OtpChallengeToken))
        {
            TempData["OtpMaskedEmail"] = result.MaskedEmail;
            return RedirectToAction(nameof(VerifyOtp), new
            {
                challengeToken = result.OtpChallengeToken,
                returnUrl
            });
        }

        if (string.IsNullOrWhiteSpace(result.Token))
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Sign-in failed.");
            return View(input);
        }

        _authService.SetAuthCookie(result.Token, result.ExpiresAt, input.RememberMe);

        if (result.MustChangePassword)
        {
            return RedirectToAction(nameof(ChangePassword));
        }

        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyOtp(string challengeToken, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return RedirectToAction(nameof(Login));
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["MaskedEmail"] = TempData["OtpMaskedEmail"] as string ?? "your email";
        TempData.Keep("OtpMaskedEmail");

        return View(new VerifyOtpInput
        {
            ChallengeToken = challengeToken,
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpInput input, CancellationToken cancellationToken)
    {
        ViewData["MaskedEmail"] = TempData["OtpMaskedEmail"] as string ?? "your email";
        TempData.Keep("OtpMaskedEmail");
        ViewData["ReturnUrl"] = input.ReturnUrl;

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var result = await _authService.VerifyOtpAsync(input, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Token))
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Verification failed.");
            return View(input);
        }

        _authService.SetAuthCookie(result.Token, result.ExpiresAt, rememberMe: false);
        TempData.Remove("OtpMaskedEmail");

        if (result.MustChangePassword)
        {
            return RedirectToAction(nameof(ChangePassword));
        }

        return RedirectToLocal(input.ReturnUrl);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string challengeToken, string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendOtpAsync(challengeToken, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.OtpChallengeToken))
        {
            TempData["OtpError"] = result.ErrorMessage ?? "Could not resend the code.";
            return RedirectToAction(nameof(Login));
        }

        TempData["OtpMaskedEmail"] = result.MaskedEmail;
        TempData["OtpInfo"] = "A new verification code was sent.";
        return RedirectToAction(nameof(VerifyOtp), new
        {
            challengeToken = result.OtpChallengeToken,
            returnUrl
        });
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordInput());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var userId = AuthClaims.GetUserId(User);
        if (userId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _authService.ChangePasswordAsync(userId.Value, input, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Token))
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not change password.");
            return View(input);
        }

        _authService.SetAuthCookie(result.Token, result.ExpiresAt, rememberMe: false);
        TempData["AuthMessage"] = "Password updated.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _authService.ClearAuthCookie();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize]
    public IActionResult LogoutGet()
    {
        _authService.ClearAuthCookie();
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}

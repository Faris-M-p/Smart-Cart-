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

    /// <summary>
    /// Shows the shared sign-in form for store admins and Super Admin.
    /// Used when an unauthenticated user opens a protected page or signs in directly.
    /// </summary>
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

    /// <summary>
    /// Validates credentials and issues a JWT cookie or redirects to OTP for Super Admin.
    /// Submitted from the login form; honors returnUrl and forced password-change flows.
    /// </summary>
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

    /// <summary>
    /// Displays the email OTP entry screen after Super Admin password success.
    /// Opened via redirect from login when RequiresOtp is returned.
    /// </summary>
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

    /// <summary>
    /// Verifies the OTP and completes Super Admin sign-in with a JWT cookie.
    /// Posted from the OTP form using the challenge token from login.
    /// </summary>
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

    /// <summary>
    /// Requests a new OTP for an in-progress Super Admin challenge.
    /// Used from the verify-OTP page when the user did not receive or expired a code.
    /// </summary>
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

    /// <summary>
    /// Shows the password change form for signed-in users.
    /// Used after login when MustChangePassword is set or from account settings.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordInput());

    /// <summary>
    /// Updates the current user's password and refreshes the auth cookie.
    /// Posted from the change-password form; requires an authenticated session.
    /// </summary>
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

    /// <summary>
    /// Placeholder page to start password recovery by email.
    /// Linked from the login screen when recovery is offered.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    /// <summary>
    /// Placeholder page to set a new password from a reset link.
    /// Used when the user follows an emailed reset token (future flow).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword() => View();

    /// <summary>
    /// Signs out by clearing the JWT auth cookie.
    /// Posted from in-app sign-out controls that use a form POST.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _authService.ClearAuthCookie();
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Signs out via GET by clearing the JWT auth cookie.
    /// Used for simple sign-out links that cannot POST anti-forgery tokens.
    /// </summary>
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

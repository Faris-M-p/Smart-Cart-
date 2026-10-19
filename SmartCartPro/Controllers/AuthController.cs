using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Shared login for store admins and platform Super Admin.
/// Super Admin password success → email OTP screen; store admin → JWT cookie.
/// All POST actions are AJAX (<c>sc-ajax.js</c>) and answer with the common JSON envelope.
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
            return Redirect(SafeReturnUrl(returnUrl));
        }

        return View(new InLoginView { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Validates credentials and issues the JWT cookie, or starts the email OTP step for Super Admin.
    /// Submitted by the login form via AJAX; the JSON <c>data.redirectUrl</c> tells the page where to go next.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(InLoginView model, CancellationToken cancellationToken)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputAuthLogin
        {
            UserNameOrEmail = model.UserNameOrEmail,
            Password = model.Password,
            RememberMe = model.RememberMe
        };
        var result = await _authService.LoginAsync(input, cancellationToken);
        if (!result.Success)
        {
            throw new ValidationException(result.ErrorMessage ?? "Sign-in failed.");
        }

        if (result.RequiresOtp && !string.IsNullOrWhiteSpace(result.OtpChallengeToken))
        {
            var otpUrl = Url.Action(nameof(VerifyOtp), "Auth", new
            {
                challengeToken = result.OtpChallengeToken,
                maskedEmail = result.MaskedEmail,
                returnUrl = model.ReturnUrl
            });

            return CommonHelpers.SuccessResponse(
                "Verification code sent. Check your email.",
                new { requiresOtp = true, redirectUrl = otpUrl });
        }

        if (string.IsNullOrWhiteSpace(result.Token))
        {
            throw new ValidationException(result.ErrorMessage ?? "Sign-in failed.");
        }

        _authService.SetAuthCookie(result.Token, result.ExpiresAt, model.RememberMe);

        var redirectUrl = result.MustChangePassword
            ? Url.Action(nameof(ChangePassword), "Auth")
            : SafeReturnUrl(model.ReturnUrl);

        return CommonHelpers.SuccessResponse(
            "Signed in successfully.",
            new { requiresOtp = false, mustChangePassword = result.MustChangePassword, redirectUrl });
    }

    /// <summary>
    /// Displays the email OTP entry screen after Super Admin password success.
    /// Opened from the login page when the JSON response says an OTP is required.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyOtp(string? challengeToken, string? maskedEmail = null, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return RedirectToAction(nameof(Login));
        }

        ViewData["MaskedEmail"] = string.IsNullOrWhiteSpace(maskedEmail) ? "your email" : maskedEmail;

        return View(new InVerifyOtpView
        {
            ChallengeToken = challengeToken,
            ReturnUrl = returnUrl
        });
    }

    /// <summary>
    /// Verifies the OTP and completes Super Admin sign-in with a JWT cookie.
    /// Posted by the OTP form via AJAX using the challenge token issued at login.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(InVerifyOtpView model, CancellationToken cancellationToken)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputAuthVerifyOtp
        {
            ChallengeToken = model.ChallengeToken,
            Code = model.Code
        };
        var result = await _authService.VerifyOtpAsync(input, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Token))
        {
            throw new ValidationException(result.ErrorMessage ?? "Verification failed.");
        }

        _authService.SetAuthCookie(result.Token, result.ExpiresAt, rememberMe: false);

        var redirectUrl = result.MustChangePassword
            ? Url.Action(nameof(ChangePassword), "Auth")
            : SafeReturnUrl(model.ReturnUrl);

        return CommonHelpers.SuccessResponse(
            "Signed in successfully.",
            new { mustChangePassword = result.MustChangePassword, redirectUrl });
    }

    /// <summary>
    /// Requests a new OTP for an in-progress Super Admin challenge.
    /// Used from the verify-OTP page when the user did not receive or expired a code; returns the new challenge token.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string? challengeToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            throw new ValidationException("The verification session is missing. Please sign in again.");
        }

        var input = new InputAuthResendOtp { ChallengeToken = challengeToken };
        var result = await _authService.ResendOtpAsync(input, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.OtpChallengeToken))
        {
            throw new ValidationException(result.ErrorMessage ?? "Could not resend the code.");
        }

        return CommonHelpers.SuccessResponse(
            "A new verification code was sent.",
            new { challengeToken = result.OtpChallengeToken, maskedEmail = result.MaskedEmail });
    }

    /// <summary>
    /// Shows the password change form for signed-in users.
    /// Used after login when MustChangePassword is set or from the header Password button.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        ViewData["MustChange"] = User.FindFirst(AuthClaims.MustChangePassword)?.Value == "1";
        return View(new InChangePasswordView());
    }

    /// <summary>
    /// Updates the current user's password and refreshes the auth cookie.
    /// Posted by the change-password form via AJAX; requires an authenticated session.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(InChangePasswordView model, CancellationToken cancellationToken)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var userId = AuthClaims.GetUserId(User);
        if (userId is null)
        {
            throw new UnauthorizedException("Your session has expired. Please sign in again.");
        }

        var input = new InputAuthChangePassword
        {
            IdUser = userId.Value,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword
        };
        var result = await _authService.ChangePasswordAsync(input, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Token))
        {
            throw new ValidationException(result.ErrorMessage ?? "Could not change password.");
        }

        _authService.SetAuthCookie(result.Token, result.ExpiresAt, rememberMe: false);

        return CommonHelpers.SuccessResponse(
            "Password updated.",
            new { redirectUrl = Url.Action("Index", "Home") });
    }

    /// <summary>
    /// Shows the form that asks for the account email or username to start password recovery.
    /// Linked from the "Forgot?" link on the login screen.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new InForgotPasswordView());

    /// <summary>
    /// Emails a time-limited reset link when the account exists; answers identically when it does not.
    /// Posted by the forgot-password form via AJAX.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(InForgotPasswordView model, CancellationToken cancellationToken)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputAuthForgotPassword
        {
            UserNameOrEmail = model.UserNameOrEmail,
            ResetPath = Url.Action(nameof(ResetPassword), "Auth") ?? "/Auth/ResetPassword",
            RequestOrigin = $"{Request.Scheme}://{Request.Host}"
        };
        var result = await _authService.RequestPasswordResetAsync(input, cancellationToken);

        return CommonHelpers.SuccessResponse(result.Message, new { sent = result.Success });
    }

    /// <summary>
    /// Shows the new-password form for an emailed reset link, or a friendly message when the link is bad or expired.
    /// Opened from the link in the reset email.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string? token, CancellationToken cancellationToken)
    {
        var input = new InputAuthResetTokenSelect { Token = token ?? string.Empty };
        var check = await _authService.GetPasswordResetTokenAsync(input, cancellationToken);
        if (!check.IsValid)
        {
            ViewData["TokenError"] = check.ErrorMessage;
        }

        return View(new InResetPasswordView { Token = check.IsValid ? input.Token : string.Empty });
    }

    /// <summary>
    /// Sets the new password from a valid reset token (single use) and sends the user to sign in.
    /// Posted by the reset-password form via AJAX.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(InResetPasswordView model, CancellationToken cancellationToken)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputAuthResetPassword
        {
            Token = model.Token,
            NewPassword = model.NewPassword
        };
        var result = await _authService.ResetPasswordAsync(input, cancellationToken);
        if (!result.Success)
        {
            throw new ValidationException(result.ErrorMessage ?? "Could not reset the password.");
        }

        return CommonHelpers.SuccessResponse(
            "Password updated. You can now sign in.",
            new { redirectUrl = Url.Action(nameof(Login), "Auth") });
    }

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
    /// Used by the header "Sign out" link, which cannot post an anti-forgery token.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult LogoutGet()
    {
        _authService.ClearAuthCookie();
        return RedirectToAction(nameof(Login));
    }

    private string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action("Index", "Home") ?? "/";
}

using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Login

/// <summary>Sign-in form (View ⇄ Controller).</summary>
public sealed class InLoginView
{
    [Display(Name = "Email or username")]
    [Required(ErrorMessage = "Enter your email or username.")]
    [StringLengthValidation(1, 200, ErrorMessage = "The email or username cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The email or username cannot contain script tags.")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Enter your password.")]
    [StringLengthValidation(1, 128, ErrorMessage = "The password cannot exceed 128 characters.")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }

    [Display(Name = "Return URL")]
    public string? ReturnUrl { get; set; }
}

/// <summary>Credentials passed Controller → Service.</summary>
public sealed class InputAuthLogin
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

#endregion

#region VerifyOtp

/// <summary>Super Admin one-time-code form (View ⇄ Controller).</summary>
public sealed class InVerifyOtpView
{
    [Display(Name = "Verification session")]
    [Required(ErrorMessage = "Something went wrong/The verification session is missing. Please sign in again.")]
    [StringLengthValidation(1, 64, ErrorMessage = "Something went wrong/The verification session is invalid. Please sign in again.")]
    public string ChallengeToken { get; set; } = string.Empty;

    [Display(Name = "One-time code")]
    [Required(ErrorMessage = "Enter the verification code from your email.")]
    [StringLengthValidation(4, 8, ErrorMessage = "The verification code must be between 4 and 8 digits.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "The verification code can contain digits only.")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Return URL")]
    public string? ReturnUrl { get; set; }
}

/// <summary>OTP check passed Controller → Service.</summary>
public sealed class InputAuthVerifyOtp
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

#endregion

#region ResendOtp

/// <summary>Resend request passed Controller → Service.</summary>
public sealed class InputAuthResendOtp
{
    public string ChallengeToken { get; set; } = string.Empty;
}

#endregion

#region ChangePassword

/// <summary>Change-password form (View ⇄ Controller).</summary>
public sealed class InChangePasswordView
{
    [Display(Name = "Current password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Enter your current password.")]
    [StringLengthValidation(1, 128, ErrorMessage = "The current password cannot exceed 128 characters.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Display(Name = "New password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Enter a new password.")]
    [StrongPassword(8, 64)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirm new password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Confirm the new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Local path to open after a successful change. Checked with Url.IsLocalUrl.</summary>
    [Display(Name = "Return address")]
    public string? ReturnUrl { get; set; }
}

/// <summary>Password change passed Controller → Service.</summary>
public sealed class InputAuthChangePassword
{
    public int IdUser { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

#endregion

#region ForgotPassword

/// <summary>Forgot-password form (View ⇄ Controller).</summary>
public sealed class InForgotPasswordView
{
    [Display(Name = "Email or username")]
    [Required(ErrorMessage = "Enter your email or username.")]
    [StringLengthValidation(1, 200, ErrorMessage = "The email or username cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The email or username cannot contain script tags.")]
    public string UserNameOrEmail { get; set; } = string.Empty;
}

/// <summary>
/// Reset request passed Controller → Service. <see cref="ResetPath"/> is the app-relative reset page path;
/// <see cref="RequestOrigin"/> is only a Development fallback (production uses <c>EnvironmentSettings:PublicBaseUrl</c>).
/// </summary>
public sealed class InputAuthForgotPassword
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string ResetPath { get; set; } = string.Empty;
    public string RequestOrigin { get; set; } = string.Empty;
}

/// <summary>Forgot-password outcome (same message whether or not the account exists).</summary>
public sealed class OutputAuthForgotPassword
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region ResetPassword

/// <summary>Reset-password form opened from the emailed link (View ⇄ Controller).</summary>
public sealed class InResetPasswordView
{
    [Display(Name = "Reset token")]
    [Required(ErrorMessage = "Something went wrong/The reset link is missing. Request a new one.")]
    public string Token { get; set; } = string.Empty;

    [Display(Name = "New password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Enter a new password.")]
    [StrongPassword(8, 64)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirm new password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Confirm the new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>Token check passed Controller → Service (used when the reset page opens).</summary>
public sealed class InputAuthResetTokenSelect
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>Result of checking a reset token.</summary>
public sealed class OutputAuthResetTokenSelect
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>New password passed Controller → Service.</summary>
public sealed class InputAuthResetPassword
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>Result of applying a reset.</summary>
public sealed class OutputAuthResetPassword
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion

#region Session

/// <summary>Result of Login / VerifyOtp / ResendOtp / ChangePassword / SwitchStore.</summary>
public sealed class OutputAuthSession
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool MustChangePassword { get; set; }
    public bool RequiresOtp { get; set; }
    public string? OtpChallengeToken { get; set; }
    public string? MaskedEmail { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public int Fk_Store { get; set; }
    public int Fk_Company { get; set; }
    public string? StoreName { get; set; }
    public string? FullName { get; set; }
}

/// <summary>Store membership projected by <c>AuthRepository</c>.</summary>
public sealed class AuthUserStoreInfo
{
    public int Fk_Store { get; set; }
    public int Fk_Company { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

#endregion

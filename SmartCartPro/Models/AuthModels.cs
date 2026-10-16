using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

public sealed class LoginInput
{
    [Required]
    [Display(Name = "Email or username")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed class VerifyOtpInput
{
    [Required]
    public string ChallengeToken { get; set; } = string.Empty;

    [Required]
    [Display(Name = "One-time code")]
    [StringLength(8, MinimumLength = 4)]
    public string Code { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public sealed class ChangePasswordInput
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    [Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class AuthSessionResult
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

public sealed class AuthUserStoreInfo
{
    public int Fk_Store { get; set; }
    public int Fk_Company { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

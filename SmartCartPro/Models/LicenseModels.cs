using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region License Update

public sealed class InLicenseUpdateView
{
    [Display(Name = "License Key")]
    [Required(ErrorMessage = "The License Key field is required.")]
    [StringLengthValidation(8, 200, ErrorMessage = "The License Key must be between 8 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The License Key cannot contain script tags.")]
    public string LicenseKey { get; set; } = string.Empty;

    [Display(Name = "License Type")]
    [Required(ErrorMessage = "The License Type field is required.")]
    [AllowedValues("standard", "premium", "enterprise", ErrorMessage = "Something went wrong/Please select a valid License Type.")]
    [StringLengthValidation(2, 50, ErrorMessage = "The License Type must be between 2 and 50 characters.")]
    [NoScriptTags(ErrorMessage = "The License Type cannot contain script tags.")]
    public string LicenseType { get; set; } = "standard";

    [Display(Name = "Status")]
    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(CommonModel.LicenseStatus), ErrorMessage = "Please select a valid status.")]
    [EnumDefined(typeof(CommonModel.LicenseStatus), ErrorMessage = "Something went wrong/Please select a valid Status.")]
    public int Status { get; set; } = (int)CommonModel.LicenseStatus.Active;

    [Display(Name = "Start Date")]
    [Required(ErrorMessage = "The Start Date field is required.")]
    public DateOnly StartDate { get; set; }

    [Display(Name = "Expiry Date")]
    [Required(ErrorMessage = "The Expiry Date field is required.")]
    [DateGreaterThanOrEqualTo("StartDate", ErrorMessage = "The Expiry Date must be greater than or equal to the Start Date.")]
    public DateOnly ExpiryDate { get; set; }

    [Display(Name = "Max Stores")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please enter a valid Max Stores value.")]
    public int MaxStores { get; set; } = 1;

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Notes cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }
}

public sealed class InputLicenseUpdate
{
    public int ActorUserId { get; set; }
    public bool CanEditLicense { get; set; }

    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = "standard";
    public int Status { get; set; } = (int)CommonModel.LicenseStatus.Active;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; } = 1;
    public string? Notes { get; set; }
}

public sealed class OutputLicenseUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public int IdLicense { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; }
    public string? Notes { get; set; }

    public bool Ok => ResponseCode == 0;
}

#endregion

#region License Select

public sealed class InputLicenseSelect
{
}

public sealed class OutputLicenseSelect
{
    public int IdLicense { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    /// <summary><see cref="CommonModel.LicenseStatus"/> integer.</summary>
    public int Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; }
    public string? Notes { get; set; }

    public string StatusLabel => Enum.IsDefined(typeof(CommonModel.LicenseStatus), Status)
        ? ((CommonModel.LicenseStatus)Status).ToString()
        : "Unknown";

    public bool IsExpired => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsNearExpiry => !IsExpired && (ExpiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays <= 30;
    public int DaysRemaining => Math.Max(0, (int)(ExpiryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DateTime.UtcNow).TotalDays);
}

#endregion

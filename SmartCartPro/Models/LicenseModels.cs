using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

public sealed class InLicenseSaveView
{
    [Display(Name = "License")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Please select a valid license.")]
    public int IdLicense { get; set; }

    [Display(Name = "Start Date")]
    [Required(ErrorMessage = "The Start Date field is required.")]
    public DateOnly StartDate { get; set; }

    [Display(Name = "Expiry Date")]
    [Required(ErrorMessage = "The Expiry Date field is required.")]
    [DateGreaterThanOrEqualTo("StartDate", ErrorMessage = "The Expiry Date must be on or after the Start Date.")]
    public DateOnly ExpiryDate { get; set; }

    [Display(Name = "Max Stores")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Max Stores must be at least 1.")]
    public int MaxStores { get; set; } = 1;

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Notes cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    [Display(Name = "Reason for change")]
    [Required(ErrorMessage = "A reason for this license change is required.")]
    [StringLengthValidation(3, 500, ErrorMessage = "The reason must be between 3 and 500 characters.")]
    [NoScriptTags(ErrorMessage = "The reason cannot contain script tags.")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class LicensePageViewModel
{
    public DateOnly Today { get; set; }
    public int ActiveStoreCount { get; set; }
    public int ActiveUserCount { get; set; }
    public bool IsCreateMode { get; set; }
    public bool IsEditMode { get; set; }
    public OutputLicenseSelect? Summary { get; set; }
    public List<OutputLicenseSelect> Licenses { get; set; } = new();
    public InLicenseSaveView? Editor { get; set; }
}

public sealed class InputLicenseCreate
{
    public int ActorUserId { get; set; }
    public bool CanEditLicense { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; }
    public string? Notes { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class InputLicenseUpdate
{
    public int ActorUserId { get; set; }
    public bool CanEditLicense { get; set; }
    public int IdLicense { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; }
    public string? Notes { get; set; }
    public string Reason { get; set; } = string.Empty;
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

public sealed class InputLicenseSelect { }

public sealed class OutputLicenseSelect
{
    public int IdLicense { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    /// <summary>Effective date/status state, retained as the legacy integer contract.</summary>
    public int Status { get; set; }
    public string StoredStatus { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; }
    public string? Notes { get; set; }
    public int ActiveStoreCount { get; set; }
    public int ActiveUserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public string? CancelledReason { get; set; }
    public DateOnly Today { get; set; }

    public string StatusLabel => Enum.IsDefined(typeof(CommonModel.LicenseStatus), Status)
        ? ((CommonModel.LicenseStatus)Status).ToString()
        : "Unknown";
    public bool IsExpired => Status == (int)CommonModel.LicenseStatus.Expired;
    public bool IsScheduled => Status == (int)CommonModel.LicenseStatus.Scheduled;
    public bool IsSuspended => Status == (int)CommonModel.LicenseStatus.Suspended;
    public int DaysRemaining => Math.Max(0, ExpiryDate.DayNumber - Today.DayNumber);
}

public sealed class InLicenseStatusView
{
    [Display(Name = "License")]
    [GreaterThanZero(ErrorMessage = "Please select a valid license.")]
    public int IdLicense { get; set; }

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "The Reason field is required.")]
    [StringLengthValidation(3, 500, ErrorMessage = "The Reason must be between 3 and 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Reason cannot contain script tags.")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class InputLicenseStatusUpdate
{
    public int ActorUserId { get; set; }
    public int IdLicense { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateOnly Today { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class OutputLicenseStatusUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdLicense { get; set; }
    public int Status { get; set; }
    public bool Cancelled { get; set; }
    public bool Ok => ResponseCode == 0;
}

using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

#region License Update

public sealed class InLicenseUpdateView
{
    [Required]
    [Display(Name = "License key")]
    public string LicenseKey { get; set; } = string.Empty;

    [Required]
    [Display(Name = "License type")]
    public string LicenseType { get; set; } = "standard";

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = "active";

    [Required]
    [Display(Name = "Start date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Display(Name = "Expiry date")]
    public DateOnly ExpiryDate { get; set; }

    [Range(1, 100)]
    [Display(Name = "Max stores")]
    public int MaxStores { get; set; } = 1;

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

public sealed class InputLicenseUpdate
{
    public int ActorUserId { get; set; }
    public bool CanEditLicense { get; set; }

    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = "standard";
    public string Status { get; set; } = "active";
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; } = 1;
    public string? Notes { get; set; }

    public static InputLicenseUpdate From(InLicenseUpdateView view, int actorUserId, bool canEditLicense) => new()
    {
        ActorUserId = actorUserId,
        CanEditLicense = canEditLicense,
        LicenseKey = view.LicenseKey,
        LicenseType = view.LicenseType,
        Status = view.Status,
        StartDate = view.StartDate,
        ExpiryDate = view.ExpiryDate,
        MaxStores = view.MaxStores,
        Notes = view.Notes
    };
}

public sealed class OutputLicenseUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public int IdLicense { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
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
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int MaxStores { get; set; }
    public string? Notes { get; set; }

    public bool IsExpired => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsNearExpiry => !IsExpired && (ExpiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays <= 30;
    public int DaysRemaining => Math.Max(0, (int)(ExpiryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DateTime.UtcNow).TotalDays);
}

#endregion

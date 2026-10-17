using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ─────────────────────────────────────────────────────────────────────────────
// Super Admin Dashboard
// ─────────────────────────────────────────────────────────────────────────────

public sealed class SuperAdminDashboardViewModel
{
    public bool OnboardingComplete { get; set; }
    public int OnboardingStep { get; set; }
    public string? CompanyName { get; set; }
    public string? LicenseStatus { get; set; }
    public DateOnly? LicenseExpiry { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public bool LicenseNearExpiry { get; set; }    // <= 30 days
    public bool LicenseExpired { get; set; }
    public bool LicenseSuspended { get; set; }
    public int EnabledModuleCount { get; set; }
    public string? Theme { get; set; }
    public string? StoreMode { get; set; }
    public int CurrentStoreCount { get; set; }
    public int MaxStores { get; set; }
    public bool AdminProvisioned { get; set; }
    public List<AuditLogRowViewModel> RecentActivity { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// Onboarding Wizard
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Master object carried through the wizard (stored in TempData as JSON).</summary>
public sealed class OnboardingDraft
{
    // Step 1 — Company Profile
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Step 2 — Regional
    public string CurrencyCode { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Locale { get; set; } = "en-IN";

    // Step 3 — Modules
    public string SelectedPresetCode { get; set; } = "BASIC";
    public List<int> SelectedModuleIds { get; set; } = new();

    // Step 4 — Theme
    public string Theme { get; set; } = "light";

    // Step 5 — License
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = "standard";
    public DateOnly LicenseStartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly LicenseExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
    public string LicenseStatus { get; set; } = "active";
    public int MaxStores { get; set; } = 1;

    // Step 6 — Store
    public string StoreMode { get; set; } = "single";
    public string StoreName { get; set; } = string.Empty;
    public string StoreCode { get; set; } = string.Empty;
    public string? StoreEmail { get; set; }
    public string? StorePhone { get; set; }
    public string? StoreAddress { get; set; }
    public string? StoreCity { get; set; }
    public string? StoreState { get; set; }
    public string? StorePostalCode { get; set; }
    public string? StoreCountry { get; set; }

    // Step 7 — Admin
    public string AdminFullName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
}

// ─ Step view models ──────────────────────────────────────────────────────────

public sealed class WizardStep1Model
{
    [Required, MaxLength(200)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Display(Name = "Company code")]
    public string CompanyCode { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Legal name")]
    public string? LegalName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Tax number")]
    public string? TaxNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Registration no.")]
    public string? RegistrationNo { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30), Phone]
    public string? Phone { get; set; }

    [MaxLength(200), Url]
    public string? Website { get; set; }

    [Display(Name = "Address line 1")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address line 2")]
    public string? AddressLine2 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }
}

public sealed class WizardStep2Model
{
    [Required, MaxLength(3)]
    [Display(Name = "Currency code")]
    public string CurrencyCode { get; set; } = "INR";

    [Required]
    [Display(Name = "Time zone")]
    public string TimeZone { get; set; } = "Asia/Kolkata";

    [Required]
    public string Locale { get; set; } = "en-IN";
}

public sealed class WizardStep3Model
{
    [Display(Name = "Preset")]
    public string SelectedPresetCode { get; set; } = "BASIC";

    public List<int> SelectedModuleIds { get; set; } = new();
    public List<ModuleCatalogItem> AllModules { get; set; } = new();
    public List<ModulePresetViewModel> Presets { get; set; } = new();
}

public sealed class ModuleCatalogItem
{
    public int IdModule { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public sealed class ModulePresetViewModel
{
    public int IdPreset { get; set; }
    public string PresetCode { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> ModuleIds { get; set; } = new();
}

public sealed class WizardStep4Model
{
    [Required]
    public string Theme { get; set; } = "light";
}

public sealed class WizardStep5Model
{
    [Required]
    [Display(Name = "License key")]
    public string LicenseKey { get; set; } = string.Empty;

    [Required]
    [Display(Name = "License type")]
    public string LicenseType { get; set; } = "standard";

    [Required]
    [Display(Name = "Start date")]
    public DateOnly LicenseStartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Display(Name = "Expiry date")]
    public DateOnly LicenseExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(1));

    [Required]
    public string LicenseStatus { get; set; } = "active";

    [Range(1, 100)]
    [Display(Name = "Max stores")]
    public int MaxStores { get; set; } = 1;
}

public sealed class WizardStep6Model
{
    [Required]
    [Display(Name = "Store mode")]
    public string StoreMode { get; set; } = "single";

    [Required, MaxLength(200)]
    [Display(Name = "Store name")]
    public string StoreName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Display(Name = "Store code")]
    public string StoreCode { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    [Display(Name = "Store email")]
    public string? StoreEmail { get; set; }

    [MaxLength(30), Phone]
    [Display(Name = "Store phone")]
    public string? StorePhone { get; set; }

    [Display(Name = "Address")]
    public string? StoreAddress { get; set; }

    public string? StoreCity { get; set; }
    public string? StoreState { get; set; }

    [Display(Name = "Postal code")]
    public string? StorePostalCode { get; set; }

    public string? StoreCountry { get; set; }

    public int MaxStores { get; set; } = 1; // from license (read-only display)
}

public sealed class WizardStep7Model
{
    [Required, MaxLength(200)]
    [Display(Name = "Full name")]
    public string AdminFullName { get; set; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    [Display(Name = "Email address")]
    public string AdminEmail { get; set; } = string.Empty;
}

public sealed class WizardReviewModel
{
    public OnboardingDraft Draft { get; set; } = new();
    public List<string> SelectedModuleNames { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// License
// ─────────────────────────────────────────────────────────────────────────────

public sealed class LicenseViewModel
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

public sealed class EditLicenseInput
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

// ─────────────────────────────────────────────────────────────────────────────
// Audit Log
// ─────────────────────────────────────────────────────────────────────────────

public sealed class AuditLogRowViewModel
{
    public int IdAuditLog { get; set; }
    public string? ActorName { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AuditLogFilterInput
{
    public string? Entity { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class AuditLogListViewModel
{
    public List<AuditLogRowViewModel> Rows { get; set; } = new();
    public AuditLogFilterInput Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Filter.PageSize);
}

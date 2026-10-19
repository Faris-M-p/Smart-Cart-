using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// SETTINGS — Phase 7
// Known keys managed via SettingController:
//   pos.invoice_prefix  (string)
//   pos.allow_negative_stock  (bool)
//   default_tax_code  (string)
//   locale.currency  (string)
//   locale.timezone  (string)
// ═══════════════════════════════════════════════════════════════════

#region Setting Select / List

public sealed class InputSettingList
{
    public string Scope { get; set; } = "company";
    public int? FkStore { get; set; }
}

public sealed class OutputSettingRow
{
    public int IdSetting { get; set; }
    public string SettingScope { get; set; } = string.Empty;
    public int? FkStore { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string ValueType { get; set; } = "string";
    public string? Description { get; set; }
}

public sealed class OutputSettingList
{
    public List<OutputSettingRow> Items { get; set; } = new();
}

#endregion

#region Setting Save (known keys only)

/// <summary>View model for the settings form. One property per known key.</summary>
public sealed class InSettingView
{
    [Display(Name = "Invoice Prefix")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Invoice Prefix cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Invoice Prefix cannot contain script tags.")]
    public string? InvoicePrefix { get; set; }

    [Display(Name = "Allow Negative Stock")]
    public bool AllowNegativeStock { get; set; }

    [Display(Name = "Default Tax Code")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Default Tax Code cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Default Tax Code cannot contain script tags.")]
    public string? DefaultTaxCode { get; set; }

    [Display(Name = "Currency Code")]
    [StringLengthValidation(0, 10, ErrorMessage = "The Currency Code cannot exceed 10 characters.")]
    [NoScriptTags(ErrorMessage = "The Currency Code cannot contain script tags.")]
    public string? Currency { get; set; }

    [Display(Name = "Timezone")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Timezone cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Timezone cannot contain script tags.")]
    public string? Timezone { get; set; }
}

public sealed class InputSettingSave
{
    public string Scope { get; set; } = "company";
    public int? FkStore { get; set; }
    /// <summary>Key → value pairs to upsert.</summary>
    public Dictionary<string, string?> Values { get; set; } = new();
    public int UpdatedBy { get; set; }
}

public sealed class OutputSettingSave
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

using System.ComponentModel.DataAnnotations;

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
    [MaxLength(50)]
    [Display(Name = "Invoice Prefix")]
    public string? InvoicePrefix { get; set; }

    [Display(Name = "Allow Negative Stock")]
    public bool AllowNegativeStock { get; set; }

    [MaxLength(50)]
    [Display(Name = "Default Tax Code")]
    public string? DefaultTaxCode { get; set; }

    [MaxLength(10)]
    [Display(Name = "Currency Code")]
    public string? Currency { get; set; }

    [MaxLength(100)]
    [Display(Name = "Timezone")]
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

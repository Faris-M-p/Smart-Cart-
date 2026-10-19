using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// Known keys managed on the Settings page (see SettingService for the catalog):
//   pos.invoice_prefix        text   company default, store override allowed
//   pos.allow_negative_stock  bool   company default, store override allowed
//   default_tax_code          tax    company default, store override allowed
//   locale.currency           text   company only
//   locale.timezone           text   company only

#region Setting List (stored rows)

public sealed class InputSettingList
{
    /// <summary>company | store.</summary>
    public string Scope { get; set; } = "company";

    /// <summary>Required when <see cref="Scope"/> is store; ignored for company.</summary>
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

#region Setting Select (known keys with effective values)

public sealed class InputSettingSelect
{
    /// <summary>company | store.</summary>
    public string Scope { get; set; } = "company";

    /// <summary>Required when <see cref="Scope"/> is store; ignored for company.</summary>
    public int? FkStore { get; set; }
}

public sealed class OutputSettingSelect
{
    public string Scope { get; set; } = "company";
    public int? FkStore { get; set; }
    public List<OutputSettingGroup> Groups { get; set; } = new();
}

public sealed class OutputSettingGroup
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-sliders";
    public string Description { get; set; } = string.Empty;
    public List<OutputSettingItem> Items { get; set; } = new();
}

public sealed class OutputSettingItem
{
    public string SettingKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    /// <summary>What the setting does, shown under the label.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>How the form renders the control: text | bool | select | suggest.</summary>
    public string Input { get; set; } = "text";

    public bool Required { get; set; }
    public int MaxLength { get; set; }
    public string? Placeholder { get; set; }

    /// <summary>Whether a store can override the company value.</summary>
    public bool AllowStoreOverride { get; set; }

    /// <summary>
    /// Value the control starts with. Company scope: the saved value, or the built-in default when nothing is saved.
    /// Store scope: the saved store override, or empty when the store uses the company value.
    /// </summary>
    public string? FormValue { get; set; }

    public string CompanyDisplay { get; set; } = string.Empty;
    public string DefaultDisplay { get; set; } = string.Empty;

    /// <summary>Value in force for the scope being edited, after falling back to company and built-in default.</summary>
    public string EffectiveValue { get; set; } = string.Empty;

    public string EffectiveDisplay { get; set; } = string.Empty;

    /// <summary>store | company | default: where <see cref="EffectiveValue"/> comes from.</summary>
    public string EffectiveSource { get; set; } = "default";

    /// <summary>Stores that override the company value (company scope only).</summary>
    public int StoreOverrideCount { get; set; }

    /// <summary>Choices for bool, select and suggest controls.</summary>
    public List<OutputSettingOption> Options { get; set; } = new();
}

public sealed class OutputSettingOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

#endregion

#region Setting Upsert

public sealed class InSettingUpsertView
{
    [Display(Name = "Scope")]
    [Required(ErrorMessage = "Something went wrong/Please select a valid scope.")]
    [AllowedValues("company", "store", ErrorMessage = "Something went wrong/Please select a valid scope.")]
    public string Scope { get; set; } = "company";

    /// <summary>Posted as Values[setting.key]. Unknown keys are rejected by the service.</summary>
    [Display(Name = "Settings")]
    public Dictionary<string, string?> Values { get; set; } = new();
}

public sealed class InputSettingUpsert
{
    public int ActorUserId { get; set; }

    /// <summary>company | store.</summary>
    public string Scope { get; set; } = "company";

    /// <summary>Store being edited (store scope only). Always taken from the signed-in user's claims.</summary>
    public int? FkStore { get; set; }

    /// <summary>Raw values posted by the form (key to value).</summary>
    public Dictionary<string, string?> Values { get; set; } = new();

    /// <summary>Validated and normalised changes prepared by the service. The repository writes only these.</summary>
    public List<InputSettingUpsertItem> Items { get; set; } = new();
}

public sealed class InputSettingUpsertItem
{
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>New value. Null at store scope removes the override so the company value applies again.</summary>
    public string? SettingValue { get; set; }

    /// <summary>string | bool. Used when the row does not exist yet.</summary>
    public string ValueType { get; set; } = "string";

    /// <summary>Used when the row does not exist yet.</summary>
    public string? Description { get; set; }
}

public sealed class OutputSettingUpsert
{
    public string Scope { get; set; } = "company";

    /// <summary>Keys whose stored value actually changed.</summary>
    public List<string> ChangedKeys { get; set; } = new();
}

#endregion

using System.Text.RegularExpressions;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Settings rules: only the known keys below can be saved, every value is validated and normalised
/// per key, store scope can override only the keys that allow it, and clearing a store value removes
/// the override so the company value applies again.
/// </summary>
public class SettingService : ISettingService
{
    private const string GroupPos = "Point of sale";
    private const string GroupLocale = "Localisation";
    private const string FieldSummary = "Please correct the highlighted fields.";

    private sealed record SettingGroupDefinition(string Title, string Icon, string Description);

    private sealed record SettingDefinition(
        string Key,
        string Group,
        string Label,
        string Description,
        string ValueType,
        string Input,
        bool Required,
        int MaxLength,
        bool AllowStoreOverride,
        string DefaultValue,
        string? Placeholder = null,
        string? Pattern = null,
        string? PatternMessage = null);

    private static readonly SettingGroupDefinition[] Groups =
    {
        new(GroupPos, "bi-receipt", "How billing behaves at the point of sale."),
        new(GroupLocale, "bi-globe2", "Currency and time used across the company.")
    };

    // Built-in defaults mirror Database-Postgres/02_Seed/003_DefaultSettings.sql.
    private static readonly SettingDefinition[] Catalog =
    {
        new(
            Key: "pos.invoice_prefix",
            Group: GroupPos,
            Label: "Invoice prefix",
            Description: "Text placed in front of every POS invoice number, for example INV. Leave it empty for no prefix. A store can use its own prefix.",
            ValueType: "string",
            Input: "text",
            Required: false,
            MaxLength: 20,
            AllowStoreOverride: true,
            DefaultValue: "INV",
            Placeholder: "INV",
            Pattern: "^[A-Za-z0-9._/-]*$",
            PatternMessage: "Use letters, numbers and the characters . _ / - only."),
        new(
            Key: "pos.allow_negative_stock",
            Group: GroupPos,
            Label: "Negative stock",
            Description: "When allowed, the POS can sell an item even if the recorded stock is zero or lower, which takes the stock below zero. When blocked, a sale that exceeds stock is rejected. A store can override this.",
            ValueType: "bool",
            Input: "bool",
            Required: true,
            MaxLength: 5,
            AllowStoreOverride: true,
            DefaultValue: "false"),
        new(
            Key: "default_tax_code",
            Group: GroupPos,
            Label: "Default tax",
            Description: "Tax profile used at the POS when a product has no tax of its own. Leave it empty to apply no default tax. A store can use a different default.",
            ValueType: "string",
            Input: "select",
            Required: false,
            MaxLength: 50,
            AllowStoreOverride: true,
            DefaultValue: string.Empty),
        new(
            Key: "locale.currency",
            Group: GroupLocale,
            Label: "Currency",
            Description: "Three-letter ISO 4217 currency code shown with amounts, for example INR or USD. Company-wide.",
            ValueType: "string",
            Input: "suggest",
            Required: true,
            MaxLength: 3,
            AllowStoreOverride: false,
            DefaultValue: "INR",
            Placeholder: "INR",
            Pattern: "^[A-Za-z]{3}$",
            PatternMessage: "Use a 3-letter currency code such as INR or USD."),
        new(
            Key: "locale.timezone",
            Group: GroupLocale,
            Label: "Timezone",
            Description: "IANA timezone used for dates, closing times and reports, for example Asia/Kolkata. Company-wide.",
            ValueType: "string",
            Input: "suggest",
            Required: true,
            MaxLength: 64,
            AllowStoreOverride: false,
            DefaultValue: "Asia/Kolkata",
            Placeholder: "Asia/Kolkata",
            Pattern: @"^[A-Za-z][A-Za-z0-9_+\-]*(/[A-Za-z0-9_+\-]+){0,2}$",
            PatternMessage: "Enter a timezone name such as Asia/Kolkata.")
    };

    private static readonly (string Value, string Label)[] CurrencySuggestions =
    {
        ("INR", "INR - Indian Rupee"), ("USD", "USD - US Dollar"), ("EUR", "EUR - Euro"),
        ("GBP", "GBP - Pound Sterling"), ("AED", "AED - UAE Dirham"), ("SAR", "SAR - Saudi Riyal"),
        ("QAR", "QAR - Qatari Riyal"), ("KWD", "KWD - Kuwaiti Dinar"), ("BHD", "BHD - Bahraini Dinar"),
        ("OMR", "OMR - Omani Rial"), ("SGD", "SGD - Singapore Dollar"), ("MYR", "MYR - Malaysian Ringgit"),
        ("AUD", "AUD - Australian Dollar"), ("NZD", "NZD - New Zealand Dollar"), ("CAD", "CAD - Canadian Dollar"),
        ("JPY", "JPY - Japanese Yen"), ("CNY", "CNY - Chinese Yuan"), ("HKD", "HKD - Hong Kong Dollar"),
        ("CHF", "CHF - Swiss Franc"), ("ZAR", "ZAR - South African Rand"), ("LKR", "LKR - Sri Lankan Rupee"),
        ("NPR", "NPR - Nepalese Rupee"), ("BDT", "BDT - Bangladeshi Taka"), ("PKR", "PKR - Pakistani Rupee")
    };

    private static readonly string[] TimezoneSuggestions =
    {
        "Asia/Kolkata", "Asia/Dubai", "Asia/Riyadh", "Asia/Qatar", "Asia/Kuwait", "Asia/Muscat",
        "Asia/Singapore", "Asia/Kuala_Lumpur", "Asia/Colombo", "Asia/Dhaka", "Asia/Karachi",
        "Asia/Kathmandu", "Asia/Tokyo", "Asia/Hong_Kong", "Australia/Sydney", "Pacific/Auckland",
        "Europe/London", "Europe/Paris", "Europe/Berlin", "America/New_York", "America/Chicago",
        "America/Denver", "America/Los_Angeles", "America/Toronto", "Africa/Johannesburg",
        "Africa/Nairobi", "UTC"
    };

    private readonly ISettingRepository _repo;
    private readonly IAuditService _audit;

    public SettingService(ISettingRepository repo, IAuditService audit)
    {
        _repo = repo;
        _audit = audit;
    }

    public Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default) =>
        _repo.ListSettingsAsync(input, ct);

    public async Task<OutputSettingSelect> GetSettingsAsync(InputSettingSelect input, CancellationToken ct = default)
    {
        var isStore = IsStoreScope(input.Scope);
        var fkStore = isStore ? input.FkStore : null;
        if (isStore && (fkStore is null or <= 0))
        {
            throw new ValidationException("Sign in as a store user to edit store settings.");
        }

        var companyRows = (await _repo.ListSettingsAsync(new InputSettingList { Scope = "company" }, ct)).Items;
        var storeRows = isStore
            ? (await _repo.ListSettingsAsync(new InputSettingList { Scope = "store", FkStore = fkStore }, ct)).Items
            : new List<OutputSettingRow>();
        var overrideCounts = isStore
            ? new Dictionary<string, int>()
            : await _repo.CountStoreOverridesAsync(ct);
        var taxes = await _repo.ListTaxOptionsAsync(ct);

        var result = new OutputSettingSelect
        {
            Scope = isStore ? "store" : "company",
            FkStore = fkStore
        };

        foreach (var group in Groups)
        {
            var items = new List<OutputSettingItem>();
            foreach (var def in Catalog.Where(d => d.Group == group.Title && (!isStore || d.AllowStoreOverride)))
            {
                items.Add(BuildItem(def, isStore, companyRows, storeRows, overrideCounts, taxes));
            }

            if (items.Count == 0) continue;

            result.Groups.Add(new OutputSettingGroup
            {
                Title = group.Title,
                Icon = group.Icon,
                Description = group.Description,
                Items = items
            });
        }

        return result;
    }

    public async Task<OutputSettingUpsert> UpsertSettingsAsync(InputSettingUpsert input, CancellationToken ct = default)
    {
        var isStore = IsStoreScope(input.Scope);
        if (isStore && (input.FkStore is null or <= 0))
        {
            throw new ValidationException("Sign in as a store user to edit store settings.");
        }

        var scope = isStore ? "store" : "company";
        int? fkStore = isStore ? input.FkStore : null;

        var current = (await _repo.ListSettingsAsync(new InputSettingList { Scope = scope, FkStore = fkStore }, ct)).Items;
        var taxes = await _repo.ListTaxOptionsAsync(ct);

        var errors = new Dictionary<string, string[]>();
        var items = new List<InputSettingUpsertItem>();
        var changes = new List<string>();

        foreach (var pair in input.Values)
        {
            var def = Catalog.FirstOrDefault(d => string.Equals(d.Key, pair.Key, StringComparison.Ordinal));
            if (def is null || (isStore && !def.AllowStoreOverride))
            {
                throw new ValidationException(
                    "One or more settings are not recognised. Reload the page and try again.",
                    $"Unknown setting key '{pair.Key}' for scope {scope}.");
            }

            var storedRow = current.FirstOrDefault(r => r.SettingKey == def.Key);
            var stored = storedRow?.SettingValue;
            var normalized = NormalizeValue(def, pair.Value, isStore, taxes, stored, out var error);
            if (error is not null)
            {
                errors[$"Values[{def.Key}]"] = new[] { error };
                continue;
            }

            // Company scope: a key without a saved value shows (and keeps) its built-in default,
            // so posting that default back is not a change. Store scope compares with the saved override only.
            var changed = isStore && normalized is null
                ? storedRow is not null
                : !string.Equals(isStore ? stored : stored ?? def.DefaultValue, normalized, StringComparison.Ordinal);
            if (!changed) continue;

            items.Add(new InputSettingUpsertItem
            {
                SettingKey = def.Key,
                SettingValue = normalized,
                ValueType = def.ValueType,
                Description = def.Label
            });
            changes.Add(isStore && normalized is null
                ? $"{def.Key}: '{stored}' -> (company value)"
                : $"{def.Key}: '{stored}' -> '{normalized}'");
        }

        ThrowIfInvalid(errors);

        if (items.Count == 0)
        {
            return new OutputSettingUpsert { Scope = scope };
        }

        input.Scope = scope;
        input.FkStore = fkStore;
        input.Items = items;

        var result = await _repo.UpsertSettingsAsync(input, ct);

        if (result.ChangedKeys.Count > 0)
        {
            var label = isStore ? $"Store {fkStore} settings" : "Company settings";
            await _audit.WriteLogAsync(new InputAuditLogCreate
            {
                ActorUserId = input.ActorUserId,
                EntityName = "Setting",
                EntityId = null,
                Action = "Update",
                Details = Truncate($"{label} updated: {string.Join("; ", changes)}", 900)
            }, ct);
        }

        return result;
    }

    public Task<string?> GetValueAsync(
        string key, string scope = "company", int? fkStore = null, CancellationToken ct = default) =>
        _repo.GetSettingValueAsync(key, scope, fkStore, ct);

    public async Task<string?> GetEffectiveValueAsync(string key, int? fkStore, CancellationToken ct = default)
    {
        var def = Catalog.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.Ordinal));

        if (fkStore is > 0 && (def is null || def.AllowStoreOverride))
        {
            var storeValue = NullIfEmpty(await _repo.GetSettingValueAsync(key, "store", fkStore, ct));
            if (storeValue is not null) return storeValue;
        }

        var companyValue = await _repo.GetSettingValueAsync(key, "company", null, ct);
        return companyValue ?? def?.DefaultValue;
    }

    // ------------------------------------------------------------------ helpers

    private static OutputSettingItem BuildItem(
        SettingDefinition def,
        bool isStore,
        List<OutputSettingRow> companyRows,
        List<OutputSettingRow> storeRows,
        Dictionary<string, int> overrideCounts,
        List<OutputSettingOption> taxes)
    {
        var companyValue = companyRows.FirstOrDefault(r => r.SettingKey == def.Key)?.SettingValue;
        var storeValue = NullIfEmpty(storeRows.FirstOrDefault(r => r.SettingKey == def.Key)?.SettingValue);
        var companyEffective = companyValue ?? def.DefaultValue;

        string effective;
        string source;
        if (isStore && storeValue is not null)
        {
            effective = storeValue;
            source = "store";
        }
        else if (companyValue is not null)
        {
            effective = companyValue;
            source = "company";
        }
        else
        {
            effective = def.DefaultValue;
            source = "default";
        }

        var formValue = isStore ? storeValue : companyEffective;
        var options = BuildOptions(def, taxes, new[] { formValue, companyEffective, effective });

        return new OutputSettingItem
        {
            SettingKey = def.Key,
            Label = def.Label,
            Description = def.Description,
            Input = def.Input,
            Required = def.Required,
            MaxLength = def.MaxLength,
            Placeholder = def.Placeholder,
            AllowStoreOverride = def.AllowStoreOverride,
            FormValue = formValue,
            CompanyDisplay = DisplayValue(def, companyEffective, options),
            DefaultDisplay = DisplayValue(def, def.DefaultValue, options),
            EffectiveValue = effective,
            EffectiveDisplay = DisplayValue(def, effective, options),
            EffectiveSource = source,
            StoreOverrideCount = overrideCounts.TryGetValue(def.Key, out var count) ? count : 0,
            Options = options
        };
    }

    private static List<OutputSettingOption> BuildOptions(
        SettingDefinition def, List<OutputSettingOption> taxes, IEnumerable<string?> valuesToKeep)
    {
        switch (def.Key)
        {
            case "pos.allow_negative_stock":
                return new List<OutputSettingOption>
                {
                    new() { Value = "true", Label = "Allowed" },
                    new() { Value = "false", Label = "Blocked" }
                };

            case "default_tax_code":
            {
                var options = taxes
                    .Select(t => new OutputSettingOption { Value = t.Value, Label = $"{t.Label} ({t.Value})" })
                    .ToList();

                // Keep a saved value that is no longer in the active tax list so the control never hides it.
                foreach (var value in valuesToKeep.Where(v => !string.IsNullOrEmpty(v)).Distinct())
                {
                    if (options.All(o => o.Value != value))
                    {
                        options.Add(new OutputSettingOption { Value = value!, Label = $"{value} (not available)" });
                    }
                }

                return options;
            }

            case "locale.currency":
                return CurrencySuggestions
                    .Select(c => new OutputSettingOption { Value = c.Value, Label = c.Label })
                    .ToList();

            case "locale.timezone":
                return TimezoneSuggestions
                    .Select(z => new OutputSettingOption { Value = z, Label = z })
                    .ToList();

            default:
                return new List<OutputSettingOption>();
        }
    }

    private static string DisplayValue(SettingDefinition def, string? value, List<OutputSettingOption> options)
    {
        if (string.IsNullOrEmpty(value)) return "None";

        if (def.Input is "bool" or "select")
        {
            var match = options.FirstOrDefault(o => string.Equals(o.Value, value, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match.Label;
        }

        return value;
    }

    /// <summary>
    /// Trims and validates one posted value. Returns the value to store. At store scope an empty value returns
    /// null (use the company value); at company scope an empty optional value returns an empty string.
    /// </summary>
    private static string? NormalizeValue(
        SettingDefinition def,
        string? raw,
        bool isStore,
        List<OutputSettingOption> taxes,
        string? stored,
        out string? error)
    {
        error = null;
        var value = raw?.Trim();

        if (string.IsNullOrEmpty(value))
        {
            if (isStore) return null;

            if (def.Required)
            {
                error = $"{def.Label} is required.";
                return null;
            }

            return string.Empty;
        }

        if (value.Length > def.MaxLength)
        {
            error = $"{def.Label} cannot exceed {def.MaxLength} characters.";
            return null;
        }

        if (def.Pattern is not null && !Regex.IsMatch(value, def.Pattern))
        {
            error = def.PatternMessage ?? $"{def.Label} has an invalid format.";
            return null;
        }

        switch (def.Key)
        {
            case "pos.allow_negative_stock":
            {
                var lower = value.ToLowerInvariant();
                if (lower is "true" or "false") return lower;

                error = "Choose Allowed or Blocked.";
                return null;
            }

            case "default_tax_code":
                if (taxes.Any(t => string.Equals(t.Value, value, StringComparison.Ordinal))
                    || string.Equals(stored, value, StringComparison.Ordinal))
                {
                    return value;
                }

                error = "The selected tax is not available.";
                return null;

            case "locale.currency":
                return value.ToUpperInvariant();

            case "locale.timezone":
                if (IsKnownTimeZone(value)) return value;

                error = "Unknown timezone. Pick one from the list or enter a valid IANA timezone name.";
                return null;

            default:
                return value;
        }
    }

    private static bool IsKnownTimeZone(string id) =>
        TimezoneSuggestions.Contains(id, StringComparer.Ordinal)
        || TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);

    private static bool IsStoreScope(string? scope) =>
        string.Equals(scope, "store", StringComparison.OrdinalIgnoreCase);

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count == 0) return;

        var developerMessage = string.Join(" ", errors.SelectMany(e => e.Value));
        throw new ValidationException(FieldSummary, developerMessage, errors);
    }
}

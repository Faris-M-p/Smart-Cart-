using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISettingService
{
    /// <summary>Stored rows for one scope (company, or one store).</summary>
    Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default);

    /// <summary>The known settings grouped for the Settings page, with saved, company and effective values.</summary>
    Task<OutputSettingSelect> GetSettingsAsync(InputSettingSelect input, CancellationToken ct = default);

    /// <summary>Validates and saves the posted values of the known keys. Unknown keys are rejected.</summary>
    Task<OutputSettingUpsert> UpsertSettingsAsync(InputSettingUpsert input, CancellationToken ct = default);

    /// <summary>Raw stored value for exactly this scope and store (no fallback). Used by other modules.</summary>
    Task<string?> GetValueAsync(string key, string scope = "company", int? fkStore = null, CancellationToken ct = default);

    /// <summary>
    /// Value in force for a store: the store override when allowed and set, else the company value,
    /// else the built-in default of a known key. Use this when behaviour depends on a setting.
    /// </summary>
    Task<string?> GetEffectiveValueAsync(string key, int? fkStore, CancellationToken ct = default);
}

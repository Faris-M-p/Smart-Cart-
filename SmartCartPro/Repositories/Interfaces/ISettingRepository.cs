using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ISettingRepository
{
    /// <summary>Stored rows for one scope (company, or one store), ordered by key then id.</summary>
    Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default);

    /// <summary>Number of stores that override each key (rows with a non-empty store value), keyed by setting key.</summary>
    Task<Dictionary<string, int>> CountStoreOverridesAsync(CancellationToken ct = default);

    /// <summary>Active, not deleted tax profiles as options: Value = tax code, Label = tax name.</summary>
    Task<List<OutputSettingOption>> ListTaxOptionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Writes the prepared items. Company scope inserts or updates the row. Store scope inserts or updates the
    /// override, and a null value removes the override row. Returns the keys that were written.
    /// </summary>
    Task<OutputSettingUpsert> UpsertSettingsAsync(InputSettingUpsert input, CancellationToken ct = default);

    /// <summary>Raw stored value for exactly this scope and store (no fallback), or null.</summary>
    Task<string?> GetSettingValueAsync(string key, string scope, int? fkStore, CancellationToken ct = default);
}

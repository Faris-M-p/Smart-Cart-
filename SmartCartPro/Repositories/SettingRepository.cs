using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Settings repository: reads and upserts key-value settings (company and store scope).
/// Entities stay internal; only Input/Output cross the boundary.
/// </summary>
public class SettingRepository : ISettingRepository
{
    private readonly SmartCartDbContext _db;

    public SettingRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default)
    {
        var isStore = string.Equals(input.Scope, "store", StringComparison.OrdinalIgnoreCase);
        var scope = isStore ? "store" : "company";

        var query = _db.Settings.AsNoTracking().Where(s => s.SettingScope == scope);

        if (isStore)
        {
            var storeId = input.FkStore ?? 0;
            query = query.Where(s => s.FkStore == storeId);
        }
        else
        {
            query = query.Where(s => s.FkStore == null);
        }

        var rows = await query
            .OrderBy(s => s.SettingKey)
            .ThenBy(s => s.IdSetting)
            .Select(s => new OutputSettingRow
            {
                IdSetting = s.IdSetting,
                SettingScope = s.SettingScope,
                FkStore = s.FkStore,
                SettingKey = s.SettingKey,
                SettingValue = s.SettingValue,
                ValueType = s.ValueType,
                Description = s.Description
            })
            .ToListAsync(ct);

        return new OutputSettingList { Items = rows };
    }

    public async Task<Dictionary<string, int>> CountStoreOverridesAsync(CancellationToken ct = default)
    {
        var rows = await _db.Settings.AsNoTracking()
            .Where(s => s.SettingScope == "store" && s.SettingValue != null && s.SettingValue != "")
            .GroupBy(s => s.SettingKey)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Key, r => r.Count);
    }

    public Task<List<OutputSettingOption>> ListTaxOptionsAsync(CancellationToken ct = default) =>
        _db.Taxes.AsNoTracking()
            .Where(t => t.IsActive && !t.Cancelled)
            .OrderBy(t => t.TaxName)
            .ThenBy(t => t.TaxCode)
            .Select(t => new OutputSettingOption { Value = t.TaxCode, Label = t.TaxName })
            .ToListAsync(ct);

    public async Task<OutputSettingUpsert> UpsertSettingsAsync(InputSettingUpsert input, CancellationToken ct = default)
    {
        var isStore = string.Equals(input.Scope, "store", StringComparison.OrdinalIgnoreCase);
        var scope = isStore ? "store" : "company";
        int? fkStore = isStore ? input.FkStore : null;

        var result = new OutputSettingUpsert { Scope = scope };
        if (input.Items.Count == 0) return result;

        var keys = input.Items.Select(i => i.SettingKey).ToList();
        var now = DateTime.UtcNow;

        // Company rows are not de-duplicated by the unique constraint (NULL store), so tolerate duplicates:
        // the lowest id is the row that readers use.
        var rows = await _db.Settings
            .Where(s => s.SettingScope == scope && s.FkStore == fkStore && keys.Contains(s.SettingKey))
            .OrderBy(s => s.IdSetting)
            .ToListAsync(ct);

        foreach (var item in input.Items)
        {
            var matches = rows.Where(r => r.SettingKey == item.SettingKey).ToList();
            var row = matches.FirstOrDefault();

            if (item.SettingValue is null)
            {
                if (isStore && matches.Count > 0)
                {
                    // Store override cleared: remove the row so the company value applies again.
                    _db.Settings.RemoveRange(matches);
                    result.ChangedKeys.Add(item.SettingKey);
                }
                else if (!isStore && row is not null && row.SettingValue is not null)
                {
                    row.SettingValue = null;
                    row.UpdatedAt = now;
                    result.ChangedKeys.Add(item.SettingKey);
                }

                continue;
            }

            if (row is null)
            {
                _db.Settings.Add(new Setting
                {
                    SettingScope = scope,
                    FkStore = fkStore,
                    SettingKey = item.SettingKey,
                    SettingValue = item.SettingValue,
                    ValueType = item.ValueType,
                    Description = item.Description,
                    CreatedAt = now
                });
                result.ChangedKeys.Add(item.SettingKey);
            }
            else if (!string.Equals(row.SettingValue, item.SettingValue, StringComparison.Ordinal))
            {
                row.SettingValue = item.SettingValue;
                row.UpdatedAt = now;
                result.ChangedKeys.Add(item.SettingKey);
            }
        }

        if (result.ChangedKeys.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return result;
    }

    public async Task<string?> GetSettingValueAsync(string key, string scope, int? fkStore, CancellationToken ct = default)
    {
        return await _db.Settings.AsNoTracking()
            .Where(s => s.SettingScope == scope && s.FkStore == fkStore && s.SettingKey == key)
            .OrderBy(s => s.IdSetting)
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync(ct);
    }
}

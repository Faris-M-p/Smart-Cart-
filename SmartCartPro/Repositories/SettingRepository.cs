using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Settings repository — reads and upserts key-value settings.
/// Entities stay internal; only Input/Output cross the boundary.
/// </summary>
public class SettingRepository : ISettingRepository
{
    private readonly SmartCartDbContext _db;

    public SettingRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default)
    {
        var q = _db.Settings.AsQueryable();

        q = q.Where(s => s.SettingScope == input.Scope);

        if (input.FkStore.HasValue)
            q = q.Where(s => s.FkStore == input.FkStore.Value);
        else
            q = q.Where(s => s.FkStore == null);

        var rows = await q
            .OrderBy(s => s.SettingKey)
            .Select(s => new OutputSettingRow
            {
                IdSetting    = s.IdSetting,
                SettingScope = s.SettingScope,
                FkStore      = s.FkStore,
                SettingKey   = s.SettingKey,
                SettingValue = s.SettingValue,
                ValueType    = s.ValueType,
                Description  = s.Description
            })
            .ToListAsync(ct);

        return new OutputSettingList { Items = rows };
    }

    public async Task<OutputSettingSave> SaveSettingsAsync(InputSettingSave input, CancellationToken ct = default)
    {
        if (input.Values.Count == 0)
            return new OutputSettingSave
                { ResponseCode = 1, ResponseMsg = "No values to save.", StatusCode = 400 };

        foreach (var (key, value) in input.Values)
        {
            var existing = await _db.Settings.FirstOrDefaultAsync(
                s => s.SettingScope == input.Scope
                  && s.FkStore == input.FkStore
                  && s.SettingKey == key, ct);

            if (existing is not null)
            {
                existing.SettingValue = value;
                existing.UpdatedAt    = DateTime.UtcNow;
            }
            else
            {
                _db.Settings.Add(new Setting
                {
                    SettingScope = input.Scope,
                    FkStore      = input.FkStore,
                    SettingKey   = key,
                    SettingValue = value,
                    ValueType    = "string",
                    CreatedAt    = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        return new OutputSettingSave
            { ResponseCode = 0, ResponseMsg = "Settings saved.", StatusCode = 200 };
    }
}

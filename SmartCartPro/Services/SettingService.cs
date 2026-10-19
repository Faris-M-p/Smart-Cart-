using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class SettingService : ISettingService
{
    private readonly ISettingRepository _repo;
    private readonly SmartCartDbContext _db;

    public SettingService(ISettingRepository repo, SmartCartDbContext db)
    {
        _repo = repo;
        _db   = db;
    }

    public Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default) =>
        _repo.ListSettingsAsync(input, ct);

    public Task<OutputSettingSave> SaveSettingsAsync(InputSettingSave input, CancellationToken ct = default) =>
        _repo.SaveSettingsAsync(input, ct);

    /// <summary>Utility: read a single setting value by key.</summary>
    public async Task<string?> GetValueAsync(
        string key, string scope = "company", int? fkStore = null, CancellationToken ct = default)
    {
        return await _db.Settings
            .Where(s => s.SettingScope == scope && s.FkStore == fkStore && s.SettingKey == key)
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync(ct);
    }
}

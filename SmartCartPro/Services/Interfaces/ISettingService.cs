using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISettingService
{
    Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default);
    Task<OutputSettingSave> SaveSettingsAsync(InputSettingSave input, CancellationToken ct = default);
    Task<string?> GetValueAsync(string key, string scope = "company", int? fkStore = null, CancellationToken ct = default);
}

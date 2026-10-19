using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ISettingRepository
{
    Task<OutputSettingList> ListSettingsAsync(InputSettingList input, CancellationToken ct = default);
    Task<OutputSettingSave> SaveSettingsAsync(InputSettingSave input, CancellationToken ct = default);
}

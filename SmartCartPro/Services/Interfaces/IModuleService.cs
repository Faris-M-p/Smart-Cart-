using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IModuleService
{
    Task<OutputModuleList> GetModuleListAsync(CancellationToken ct = default);
    Task<OutputModuleAssign> AssignModulesAsync(InputModuleAssign input, CancellationToken ct = default);
    Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default);
}

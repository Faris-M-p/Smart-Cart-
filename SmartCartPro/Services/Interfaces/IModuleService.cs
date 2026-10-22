using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IModuleService
{
    Task<OutputModuleList> GetModuleListAsync(CancellationToken ct = default);
    Task<OutputModuleAssign> AssignModulesAsync(InputModuleAssign input, CancellationToken ct = default);
    Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default);
    Task<OutputModuleFeatureTree> GetFeatureTreeAsync(CancellationToken ct = default);
    Task<OutputModuleFeatureAssign> AssignFeaturesAsync(InputModuleFeatureAssign input, CancellationToken ct = default);
    Task<OutputFeatureGate> GetFeatureGateAsync(CancellationToken ct = default);
}

using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IModuleRepository
{
    Task<List<OutputModuleSelect>> ListAllModulesAsync(CancellationToken ct = default);
    Task<List<OutputModulePresetSelect>> ListPresetsAsync(CancellationToken ct = default);
    Task<List<int>> GetEnabledModuleIdsAsync(CancellationToken ct = default);
    Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default);
    Task<OutputModuleAssign> SaveModuleAssignmentAsync(InputModuleAssign input, CancellationToken ct = default);
}

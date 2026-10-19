using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _repo;

    public ModuleService(IModuleRepository repo) => _repo = repo;

    public async Task<OutputModuleList> GetModuleListAsync(CancellationToken ct = default)
    {
        var modules = await _repo.ListAllModulesAsync(ct);
        var enabledIds = await _repo.GetEnabledModuleIdsAsync(ct);
        var presets = await _repo.ListPresetsAsync(ct);

        var enabledSet = enabledIds.ToHashSet();
        foreach (var m in modules)
        {
            m.IsEnabled = enabledSet.Contains(m.IdModule);
        }

        return new OutputModuleList
        {
            Modules = modules,
            EnabledModuleIds = enabledIds,
            Presets = presets
        };
    }

    public Task<OutputModuleAssign> AssignModulesAsync(InputModuleAssign input, CancellationToken ct = default) =>
        _repo.SaveModuleAssignmentAsync(input, ct);

    public Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default) =>
        _repo.GetEnabledModuleCodesAsync(ct);
}

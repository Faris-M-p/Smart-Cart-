using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class ModuleService : IModuleService
{
    private static readonly Dictionary<string, string> ModuleDescriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DASHBOARD"] = "Home metrics and shortcuts for the signed-in store.",
        ["PRODUCTS"] = "Catalog: products, categories, brands, units and taxes.",
        ["INVENTORY"] = "Stock on hand, adjustments, transfers and history.",
        ["PURCHASES"] = "Supplier bills, receiving and purchase documents.",
        ["POS"] = "In-store sales, returns, invoices and cash sessions.",
        ["CUSTOMERS"] = "Customer list and profiles used by POS and online orders.",
        ["REPORTS"] = "Sales, stock and tax reports.",
        ["SETTINGS"] = "Company profile, stores and system settings.",
        ["USERS"] = "Employees, roles and permission assignment."
    };

    private readonly IModuleRepository _repo;
    private readonly IAuditService _audit;

    public ModuleService(IModuleRepository repo, IAuditService audit)
    {
        _repo = repo;
        _audit = audit;
    }

    public async Task<OutputModuleList> GetModuleListAsync(CancellationToken ct = default)
    {
        var modules = await _repo.ListAllModulesAsync(ct);
        var enabledIds = await _repo.GetEnabledModuleIdsAsync(ct);
        var presets = await _repo.ListPresetsAsync(ct);
        var coreIds = CoreModuleIds(presets, modules);

        var enabledSet = enabledIds.ToHashSet();
        foreach (var m in modules)
        {
            m.IsEnabled = enabledSet.Contains(m.IdModule);
            m.IsCore = coreIds.Contains(m.IdModule);
            m.Description = ModuleDescriptions.TryGetValue(m.ModuleCode, out var description)
                ? description
                : m.ModuleName;
        }

        return new OutputModuleList
        {
            Modules = modules,
            EnabledModuleIds = enabledIds,
            Presets = presets
        };
    }

    public async Task<OutputModuleAssign> AssignModulesAsync(InputModuleAssign input, CancellationToken ct = default)
    {
        var data = await GetModuleListAsync(ct);
        var knownIds = data.Modules.Select(m => m.IdModule).ToHashSet();
        var coreIds = data.Modules.Where(m => m.IsCore).Select(m => m.IdModule).ToList();

        var enabled = (input.EnabledModuleIds ?? new List<int>())
            .Where(knownIds.Contains)
            .Distinct()
            .ToList();

        foreach (var coreId in coreIds)
        {
            if (!enabled.Contains(coreId))
            {
                enabled.Add(coreId);
            }
        }

        input.EnabledModuleIds = enabled;
        var result = await _repo.SaveModuleAssignmentAsync(input, ct);

        await _audit.WriteLogAsync(new InputAuditLogCreate
        {
            ActorUserId = input.ActorUserId,
            EntityName = "Module",
            EntityId = null,
            Action = "Update",
            Details = $"Module assignment saved ({result.EnabledCount} modules enabled)."
        }, ct);

        return result;
    }

    public Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default) =>
        _repo.GetEnabledModuleCodesAsync(ct);

    private static HashSet<int> CoreModuleIds(
        List<OutputModulePresetSelect> presets,
        List<OutputModuleSelect> modules)
    {
        var basic = presets.FirstOrDefault(p =>
            string.Equals(p.PresetCode, "BASIC", StringComparison.OrdinalIgnoreCase));
        if (basic is not null && basic.ModuleIds.Count > 0)
        {
            return basic.ModuleIds.ToHashSet();
        }

        return modules
            .Where(m => string.Equals(m.ModuleCode, "DASHBOARD", StringComparison.OrdinalIgnoreCase))
            .Select(m => m.IdModule)
            .ToHashSet();
    }
}

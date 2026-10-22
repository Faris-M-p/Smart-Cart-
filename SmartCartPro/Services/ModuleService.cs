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
        ["REPORTS"] = "Sales, stock, tax and purchase reports.",
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
        var enabledSet = enabledIds.ToHashSet();

        foreach (var m in modules)
        {
            m.IsEnabled = enabledSet.Contains(m.IdModule) || m.IsCore;
            if (string.IsNullOrWhiteSpace(m.Description)
                && ModuleDescriptions.TryGetValue(m.ModuleCode, out var fallback))
            {
                m.Description = fallback;
            }
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
        var rows = await _repo.ListFeatureRowsAsync(ct);
        if (rows.Count == 0)
        {
            var legacy = await AssignModulesLegacyAsync(input, ct);
            return legacy;
        }

        var modules = rows
            .GroupBy(r => r.IdModule)
            .Select(g => g.First())
            .ToList();
        var known = modules.Select(m => m.IdModule).ToHashSet();
        var requested = (input.EnabledModuleIds ?? new List<int>())
            .Where(known.Contains)
            .ToHashSet();

        foreach (var core in modules.Where(m => m.ModuleIsCore))
            requested.Add(core.IdModule);

        var leaves = rows.Where(r => !r.IsGroup).ToList();
        var codes = leaves
            .Where(r => requested.Contains(r.IdModule))
            .Select(r => r.FeatureCode)
            .ToList();

        var featureInput = new InputModuleFeatureAssign
        {
            ActorUserId = input.ActorUserId,
            EnabledFeatureCodes = codes
        };
        var saved = await AssignFeaturesAsync(featureInput, ct);
        return new OutputModuleAssign
        {
            ResponseCode = saved.ResponseCode,
            ResponseMsg = saved.ResponseMsg,
            StatusCode = saved.StatusCode,
            EnabledCount = modules.Count(m => requested.Contains(m.IdModule) || m.ModuleIsCore)
        };
    }

    public Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default) =>
        _repo.GetEnabledModuleCodesAsync(ct);

    public async Task<OutputModuleFeatureTree> GetFeatureTreeAsync(CancellationToken ct = default)
    {
        var rows = await _repo.ListFeatureRowsAsync(ct);
        var presets = await _repo.ListFeaturePresetsAsync(ct);
        return BuildTree(rows, presets);
    }

    public async Task<OutputModuleFeatureAssign> AssignFeaturesAsync(InputModuleFeatureAssign input, CancellationToken ct = default)
    {
        var rows = await _repo.ListFeatureRowsAsync(ct);
        var before = rows.Where(r => !r.IsGroup && r.IsEnabled)
            .Select(r => r.FeatureCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var normalized = Normalize(rows, input.EnabledFeatureCodes);
        var leaves = rows.Where(r => !r.IsGroup).ToList();
        var byCode = leaves.ToDictionary(r => r.FeatureCode, StringComparer.OrdinalIgnoreCase);
        var featureIds = normalized
            .Where(byCode.ContainsKey)
            .Select(code => byCode[code].IdFeature)
            .Distinct()
            .ToList();
        var moduleIds = leaves
            .Where(r => normalized.Contains(r.FeatureCode))
            .Select(r => r.IdModule)
            .Distinct()
            .ToList();

        await _repo.SaveFeatureAssignmentAsync(featureIds, moduleIds, ct);

        var added = normalized.Where(code => !before.Contains(code)).OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        var removed = before.Where(code => !normalized.Contains(code)).OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        var details = "Added: " + (added.Count == 0 ? "none" : string.Join(", ", added))
            + ". Removed: " + (removed.Count == 0 ? "none" : string.Join(", ", removed)) + ".";

        await _audit.WriteLogAsync(new InputAuditLogCreate
        {
            ActorUserId = input.ActorUserId,
            EntityName = "Module",
            EntityId = null,
            Action = "Update",
            Details = details
        }, ct);

        foreach (var row in rows)
            row.IsEnabled = !row.IsGroup && normalized.Contains(row.FeatureCode);

        var tree = BuildTree(rows, await _repo.ListFeaturePresetsAsync(ct));
        return new OutputModuleFeatureAssign
        {
            ResponseCode = 0,
            ResponseMsg = "Feature assignment saved.",
            StatusCode = 200,
            EnabledCount = normalized.Count,
            EnabledFeatureCodes = normalized.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList(),
            AddedFeatureCodes = added,
            RemovedFeatureCodes = removed,
            TotalLeaves = tree.TotalLeaves,
            EnabledLeaves = tree.EnabledLeaves,
            DisabledLeaves = tree.DisabledLeaves,
            CoreLeaves = tree.CoreLeaves
        };
    }

    public async Task<OutputFeatureGate> GetFeatureGateAsync(CancellationToken ct = default)
    {
        var rows = await _repo.ListFeatureRowsAsync(ct);
        if (rows.Count == 0)
        {
            return new OutputFeatureGate
            {
                CatalogReady = false,
                EnabledModuleCodes = await _repo.GetEnabledModuleCodesAsync(ct)
            };
        }

        var enabledLeaves = rows.Where(r => !r.IsGroup && r.IsEnabled).ToList();
        var enabledModules = enabledLeaves
            .Select(r => r.ModuleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new OutputFeatureGate
        {
            CatalogReady = true,
            EnabledModuleCodes = enabledModules,
            EnabledFeatureCodes = enabledLeaves.Select(r => r.FeatureCode).ToList(),
            CompanyAdminCoreFeatureCodes = rows
                .Where(r => !r.IsGroup && r.IsCore && IsCompanyAdminModule(r.ModuleCode))
                .Select(r => r.FeatureCode)
                .ToList()
        };
    }

    internal static HashSet<string> Normalize(IReadOnlyList<OutputModuleFeatureRow> rows, IEnumerable<string>? requestedCodes)
    {
        var leaves = rows.Where(r => !r.IsGroup).ToList();
        var known = leaves.ToDictionary(r => r.FeatureCode, StringComparer.OrdinalIgnoreCase);
        var requested = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (requestedCodes is not null)
        {
            foreach (var code in requestedCodes)
            {
                if (!string.IsNullOrWhiteSpace(code) && known.ContainsKey(code))
                    requested.Add(known[code].FeatureCode);
            }
        }

        var enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in leaves.GroupBy(r => r.ModuleCode, StringComparer.OrdinalIgnoreCase))
        {
            var moduleLeaves = module.ToList();
            var moduleIsCore = moduleLeaves[0].ModuleIsCore;
            var anyRequested = moduleLeaves.Any(r => requested.Contains(r.FeatureCode));
            if (!moduleIsCore && !anyRequested)
                continue;

            foreach (var leaf in moduleLeaves.Where(r => r.IsCore))
                enabled.Add(leaf.FeatureCode);

            foreach (var leaf in moduleLeaves.Where(r => !r.IsCore && requested.Contains(r.FeatureCode)))
                enabled.Add(leaf.FeatureCode);
        }

        return enabled;
    }

    private async Task<OutputModuleAssign> AssignModulesLegacyAsync(InputModuleAssign input, CancellationToken ct)
    {
        var data = await GetModuleListAsync(ct);
        var knownIds = data.Modules.Select(m => m.IdModule).ToHashSet();
        var enabled = (input.EnabledModuleIds ?? new List<int>())
            .Where(knownIds.Contains)
            .Distinct()
            .ToList();

        foreach (var core in data.Modules.Where(m => m.IsCore))
        {
            if (!enabled.Contains(core.IdModule))
                enabled.Add(core.IdModule);
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

    private static OutputModuleFeatureTree BuildTree(
        List<OutputModuleFeatureRow> rows,
        List<OutputModuleFeaturePreset> presets)
    {
        var leaves = rows.Where(r => !r.IsGroup).ToList();
        var enabled = leaves.Where(r => r.IsEnabled).Select(r => r.FeatureCode).ToList();
        var modules = rows
            .GroupBy(r => r.IdModule)
            .OrderBy(g => g.First().ModuleOrder)
            .Select(g =>
            {
                var head = g.First();
                var description = head.ModuleDescription;
                if (string.IsNullOrWhiteSpace(description)
                    && ModuleDescriptions.TryGetValue(head.ModuleCode, out var fallback))
                {
                    description = fallback;
                }

                return new OutputModuleFeatureModule
                {
                    IdModule = head.IdModule,
                    ModuleCode = head.ModuleCode,
                    ModuleName = head.ModuleName,
                    Description = description,
                    IsCore = head.ModuleIsCore,
                    DisplayOrder = head.ModuleOrder,
                    Features = BuildChildren(g.ToList(), null)
                };
            })
            .ToList();

        return new OutputModuleFeatureTree
        {
            Modules = modules,
            Presets = presets,
            EnabledFeatureCodes = enabled,
            TotalLeaves = leaves.Count,
            EnabledLeaves = leaves.Count(r => r.IsEnabled),
            DisabledLeaves = leaves.Count(r => !r.IsEnabled),
            CoreLeaves = leaves.Count(r => r.IsCore)
        };
    }

    private static List<OutputModuleFeatureNode> BuildChildren(List<OutputModuleFeatureRow> rows, int? parentId)
    {
        return rows
            .Where(r => r.ParentId == parentId)
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.FeatureName)
            .Select(r => new OutputModuleFeatureNode
            {
                IdFeature = r.IdFeature,
                FeatureCode = r.FeatureCode,
                FeatureName = r.FeatureName,
                Description = r.Description,
                IsCore = r.IsCore,
                IsGroup = r.IsGroup,
                DisplayOrder = r.DisplayOrder,
                Children = BuildChildren(rows, r.IdFeature)
            })
            .ToList();
    }

    private static bool IsCompanyAdminModule(string moduleCode) =>
        string.Equals(moduleCode, "USERS", StringComparison.OrdinalIgnoreCase)
        || string.Equals(moduleCode, "SETTINGS", StringComparison.OrdinalIgnoreCase);
}

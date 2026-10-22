using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly SmartCartDbContext _db;

    public ModuleRepository(SmartCartDbContext db) => _db = db;

    public Task<List<OutputModuleSelect>> ListAllModulesAsync(CancellationToken ct = default) =>
        _db.Modules.AsNoTracking()
            .Where(m => m.IsActive && !m.Cancelled)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.ModuleName)
            .Select(m => new OutputModuleSelect
            {
                IdModule = m.IdModule,
                ModuleCode = m.ModuleCode,
                ModuleName = m.ModuleName,
                Description = m.Description ?? string.Empty,
                DisplayOrder = m.DisplayOrder,
                IsCore = m.IsCore
            })
            .ToListAsync(ct);

    public async Task<List<OutputModulePresetSelect>> ListPresetsAsync(CancellationToken ct = default)
    {
        var presets = await _db.ModulePresets.AsNoTracking()
            .Include(p => p.Items)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);

        return presets.Select(p => new OutputModulePresetSelect
        {
            IdPreset = p.IdPreset,
            PresetCode = p.PresetCode,
            PresetName = p.PresetName,
            Description = p.Description,
            DisplayOrder = p.DisplayOrder,
            ModuleIds = p.Items.Select(i => i.FkModule).ToList()
        }).ToList();
    }

    public Task<List<int>> GetEnabledModuleIdsAsync(CancellationToken ct = default) =>
        _db.InstallationModules.AsNoTracking()
            .Select(im => im.FkModule)
            .ToListAsync(ct);

    public Task<List<string>> GetEnabledModuleCodesAsync(CancellationToken ct = default) =>
        _db.InstallationModules.AsNoTracking()
            .Join(_db.Modules, im => im.FkModule, m => m.IdModule, (im, m) => m.ModuleCode)
            .ToListAsync(ct);

    public async Task<OutputModuleAssign> SaveModuleAssignmentAsync(InputModuleAssign input, CancellationToken ct = default)
    {
        // Remove all existing entries
        var existing = await _db.InstallationModules.ToListAsync(ct);
        _db.InstallationModules.RemoveRange(existing);

        // Insert the new set
        var now = DateTime.UtcNow;
        foreach (var moduleId in input.EnabledModuleIds.Distinct())
        {
            _db.InstallationModules.Add(new InstallationModule
            {
                FkModule = moduleId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);

        return new OutputModuleAssign
        {
            ResponseCode = 0,
            ResponseMsg = "Module assignment saved.",
            StatusCode = 200,
            EnabledCount = input.EnabledModuleIds.Distinct().Count()
        };
    }

    public async Task<List<OutputModuleFeatureRow>> ListFeatureRowsAsync(CancellationToken ct = default)
    {
        var enabledIds = (await _db.InstallationFeatures.AsNoTracking()
            .Select(ix => ix.FkFeature)
            .ToListAsync(ct)).ToHashSet();

        var rows = await (
            from f in _db.ModuleFeatures.AsNoTracking()
            join m in _db.Modules.AsNoTracking() on f.FkModule equals m.IdModule
            where f.IsActive && !f.Cancelled && m.IsActive && !m.Cancelled
            select new OutputModuleFeatureRow
            {
                IdFeature = f.IdFeature,
                ParentId = f.FkParentFeature,
                IdModule = m.IdModule,
                ModuleCode = m.ModuleCode,
                ModuleName = m.ModuleName,
                ModuleDescription = m.Description ?? string.Empty,
                ModuleIsCore = m.IsCore,
                ModuleOrder = m.DisplayOrder,
                FeatureCode = f.FeatureCode,
                FeatureName = f.FeatureName,
                Description = f.Description ?? string.Empty,
                IsCore = f.IsCore,
                DisplayOrder = f.DisplayOrder
            }).ToListAsync(ct);

        foreach (var row in rows)
            row.IsEnabled = enabledIds.Contains(row.IdFeature);

        var parentIds = rows.Where(r => r.ParentId is not null).Select(r => r.ParentId!.Value).ToHashSet();
        foreach (var row in rows)
            row.IsGroup = parentIds.Contains(row.IdFeature);

        return rows
            .OrderBy(r => r.ModuleOrder)
            .ThenBy(r => r.DisplayOrder)
            .ThenBy(r => r.FeatureName)
            .ToList();
    }

    public async Task<List<OutputModuleFeaturePreset>> ListFeaturePresetsAsync(CancellationToken ct = default)
    {
        var presets = await _db.ModulePresets.AsNoTracking()
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new OutputModuleFeaturePreset
            {
                PresetCode = p.PresetCode,
                PresetName = p.PresetName,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(ct);

        var links = await (
            from link in _db.ModulePresetFeatures.AsNoTracking()
            join preset in _db.ModulePresets.AsNoTracking() on link.FkPreset equals preset.IdPreset
            join feature in _db.ModuleFeatures.AsNoTracking() on link.FkFeature equals feature.IdFeature
            where !feature.Cancelled && feature.IsActive
            select new { preset.PresetCode, feature.FeatureCode }
        ).ToListAsync(ct);

        var byPreset = links
            .GroupBy(x => x.PresetCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(x => x.FeatureCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var preset in presets)
        {
            if (byPreset.TryGetValue(preset.PresetCode, out var codes))
                preset.FeatureCodes = codes;
        }

        return presets;
    }

    public async Task SaveFeatureAssignmentAsync(
        IReadOnlyCollection<int> featureIds,
        IReadOnlyCollection<int> moduleIds,
        CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var existingFeatures = await _db.InstallationFeatures.ToListAsync(ct);
        _db.InstallationFeatures.RemoveRange(existingFeatures);

        var existingModules = await _db.InstallationModules.ToListAsync(ct);
        _db.InstallationModules.RemoveRange(existingModules);

        var now = DateTime.UtcNow;
        foreach (var featureId in featureIds.Distinct())
        {
            _db.InstallationFeatures.Add(new InstallationFeature
            {
                FkFeature = featureId,
                CreatedAt = now
            });
        }

        foreach (var moduleId in moduleIds.Distinct())
        {
            _db.InstallationModules.Add(new InstallationModule
            {
                FkModule = moduleId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}

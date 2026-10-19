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
                DisplayOrder = m.DisplayOrder
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
}

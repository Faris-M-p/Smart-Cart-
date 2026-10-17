using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class SuperAdminRepository : ISuperAdminRepository
{
    private readonly SmartCartDbContext _db;

    public SuperAdminRepository(SmartCartDbContext db) => _db = db;

    // ── Company ───────────────────────────────────────────────────────────────

    public Task<Company?> GetCompanyAsync(CancellationToken ct = default) =>
        _db.Companies.FirstOrDefaultAsync(c => !c.Cancelled, ct);

    public async Task<Company> UpsertCompanyAsync(Company company, CancellationToken ct = default)
    {
        var existing = await _db.Companies.FirstOrDefaultAsync(c => !c.Cancelled, ct);
        if (existing is null)
        {
            company.CreatedAt = DateTime.UtcNow;
            _db.Companies.Add(company);
        }
        else
        {
            existing.CompanyName    = company.CompanyName;
            existing.CompanyCode    = company.CompanyCode;
            existing.LegalName      = company.LegalName;
            existing.TaxNumber      = company.TaxNumber;
            existing.RegistrationNo = company.RegistrationNo;
            existing.Email          = company.Email;
            existing.Phone          = company.Phone;
            existing.Website        = company.Website;
            existing.AddressLine1   = company.AddressLine1;
            existing.AddressLine2   = company.AddressLine2;
            existing.City           = company.City;
            existing.State          = company.State;
            existing.PostalCode     = company.PostalCode;
            existing.Country        = company.Country;
            existing.CurrencyCode   = company.CurrencyCode;
            existing.TimeZone       = company.TimeZone;
            existing.Locale         = company.Locale;
            existing.UpdatedAt      = DateTime.UtcNow;
            company = existing;
        }
        await _db.SaveChangesAsync(ct);
        return company;
    }

    // ── Store ─────────────────────────────────────────────────────────────────

    public Task<Store?> GetDefaultStoreAsync(CancellationToken ct = default) =>
        _db.Stores.FirstOrDefaultAsync(s => s.IsDefault && !s.Cancelled, ct);

    public Task<int> GetActiveStoreCountAsync(CancellationToken ct = default) =>
        _db.Stores.CountAsync(s => s.IsActive && !s.Cancelled, ct);

    public async Task<Store> CreateStoreAsync(Store store, CancellationToken ct = default)
    {
        store.CreatedAt = DateTime.UtcNow;
        _db.Stores.Add(store);
        await _db.SaveChangesAsync(ct);
        return store;
    }

    // ── License ───────────────────────────────────────────────────────────────

    public Task<License?> GetLicenseAsync(CancellationToken ct = default) =>
        _db.Licenses.FirstOrDefaultAsync(l => !l.Cancelled, ct);

    public async Task<License> UpsertLicenseAsync(License license, CancellationToken ct = default)
    {
        var existing = await _db.Licenses.FirstOrDefaultAsync(l => !l.Cancelled, ct);
        if (existing is null)
        {
            license.CreatedAt = DateTime.UtcNow;
            _db.Licenses.Add(license);
        }
        else
        {
            existing.LicenseKey  = license.LicenseKey;
            existing.LicenseType = license.LicenseType;
            existing.Status      = license.Status;
            existing.StartDate   = license.StartDate;
            existing.ExpiryDate  = license.ExpiryDate;
            existing.MaxStores   = license.MaxStores;
            existing.Notes       = license.Notes;
            existing.UpdatedAt   = DateTime.UtcNow;
            license = existing;
        }
        await _db.SaveChangesAsync(ct);
        return license;
    }

    // ── Installation config ───────────────────────────────────────────────────

    public Task<InstallationConfig?> GetInstallationConfigAsync(CancellationToken ct = default) =>
        _db.InstallationConfigs
           .Include(c => c.License)
           .FirstOrDefaultAsync(ct);

    public async Task<InstallationConfig> UpsertInstallationConfigAsync(
        InstallationConfig config, CancellationToken ct = default)
    {
        var existing = await _db.InstallationConfigs.FirstOrDefaultAsync(ct);
        if (existing is null)
        {
            config.CreatedAt = DateTime.UtcNow;
            _db.InstallationConfigs.Add(config);
        }
        else
        {
            existing.FkLicense          = config.FkLicense;
            existing.Theme              = config.Theme;
            existing.StoreMode          = config.StoreMode;
            existing.OnboardingStep     = config.OnboardingStep;
            existing.OnboardingComplete = config.OnboardingComplete;
            existing.ActivatedAt        = config.ActivatedAt;
            existing.UpdatedAt          = DateTime.UtcNow;
            config = existing;
        }
        await _db.SaveChangesAsync(ct);
        return config;
    }

    // ── Modules ───────────────────────────────────────────────────────────────

    public Task<List<Module>> GetAllActiveModulesAsync(CancellationToken ct = default) =>
        _db.Modules
           .Where(m => m.IsActive && !m.Cancelled)
           .OrderBy(m => m.DisplayOrder)
           .ToListAsync(ct);

    public Task<List<ModulePreset>> GetPresetsWithItemsAsync(CancellationToken ct = default) =>
        _db.ModulePresets
           .Include(p => p.Items)
               .ThenInclude(i => i.Module)
           .OrderBy(p => p.DisplayOrder)
           .ToListAsync(ct);

    public Task<List<int>> GetEnabledModuleIdsAsync(CancellationToken ct = default) =>
        _db.InstallationModules
           .Select(im => im.FkModule)
           .ToListAsync(ct);

    public async Task SetEnabledModulesAsync(IEnumerable<int> moduleIds, CancellationToken ct = default)
    {
        // Validate against the known active modules
        var validIds = await _db.Modules
            .Where(m => m.IsActive && !m.Cancelled)
            .Select(m => m.IdModule)
            .ToListAsync(ct);

        var toSet = moduleIds.Where(id => validIds.Contains(id)).Distinct().ToList();

        var existing = await _db.InstallationModules.ToListAsync(ct);
        _db.InstallationModules.RemoveRange(existing);

        foreach (var id in toSet)
        {
            _db.InstallationModules.Add(new InstallationModule
            {
                FkModule  = id,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    public Task<Role?> GetRoleByCodeAsync(string roleCode, CancellationToken ct = default) =>
        _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == roleCode && !r.Cancelled, ct);

    // ── Users ─────────────────────────────────────────────────────────────────

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var lower = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == lower && !u.Cancelled, ct);
    }

    public async Task<User> CreateUserAsync(User user, CancellationToken ct = default)
    {
        user.CreatedAt = DateTime.UtcNow;
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task AssignRoleAsync(int idUser, int idRole, CancellationToken ct = default)
    {
        var exists = await _db.UserRoles.AnyAsync(
            ur => ur.FkUser == idUser && ur.FkRole == idRole, ct);
        if (exists) return;

        _db.UserRoles.Add(new UserRole
        {
            FkUser    = idUser,
            FkRole    = idRole,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task GrantStoreAccessAsync(int idUser, int idStore, bool isDefault, CancellationToken ct = default)
    {
        var exists = await _db.UserStoreAccess.AnyAsync(
            a => a.FkUser == idUser && a.FkStore == idStore, ct);
        if (exists) return;

        _db.UserStoreAccess.Add(new UserStoreAccess
        {
            FkUser    = idUser,
            FkStore   = idStore,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> HasCompanyAdminAsync(CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => !u.IsPlatformSuperAdmin && u.IsActive && !u.Cancelled, ct);

    // ── Invitations ───────────────────────────────────────────────────────────

    public async Task<UserInvitation> CreateInvitationAsync(UserInvitation invitation, CancellationToken ct = default)
    {
        invitation.CreatedAt = DateTime.UtcNow;
        _db.UserInvitations.Add(invitation);
        await _db.SaveChangesAsync(ct);
        return invitation;
    }

    public Task<UserInvitation?> GetInvitationByTokenAsync(string token, CancellationToken ct = default) =>
        _db.UserInvitations
           .Include(i => i.Role)
           .Include(i => i.Store)
           .FirstOrDefaultAsync(i => i.InvitationToken == token, ct);

    public async Task UpdateInvitationAsync(UserInvitation invitation, CancellationToken ct = default)
    {
        invitation.UpdatedAt = DateTime.UtcNow;
        _db.UserInvitations.Update(invitation);
        await _db.SaveChangesAsync(ct);
    }

    public Task<UserInvitation?> GetLatestInvitationAsync(CancellationToken ct = default) =>
        _db.UserInvitations
           .OrderByDescending(i => i.CreatedAt)
           .FirstOrDefaultAsync(ct);

    // ── Audit Log ─────────────────────────────────────────────────────────────

    public async Task AddAuditLogAsync(AuditLog log, CancellationToken ct = default)
    {
        log.CreatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(List<AuditLog> Rows, int TotalCount)> GetAuditLogsAsync(
        AuditLogFilterInput filter, CancellationToken ct = default)
    {
        var q = _db.AuditLogs
                   .Include(a => a.User)
                   .AsNoTracking()
                   .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Entity))
            q = q.Where(a => a.EntityName.ToLower().Contains(filter.Entity.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Action))
            q = q.Where(a => a.Action.ToLower().Contains(filter.Action.ToLower()));

        if (filter.From.HasValue)
            q = q.Where(a => a.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            q = q.Where(a => a.CreatedAt <= filter.To.Value);

        var total = await q.CountAsync(ct);
        var skip  = (filter.Page - 1) * filter.PageSize;

        var rows = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (rows, total);
    }
}

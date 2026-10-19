using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>Roles and their permission sets. EF/LINQ only; deletes are soft (cancelled flag).</summary>
public class RoleRepository : IRoleRepository
{
    private readonly SmartCartDbContext _db;

    public RoleRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = input.PageSize <= 0 ? 25 : Math.Min(input.PageSize, 100);
        var sortBy = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();

        var query = _db.Roles.AsNoTracking().Where(r => !r.Cancelled);

        if (input.ActiveOnly == true)
        {
            query = query.Where(r => r.IsActive);
        }
        else if (input.ActiveOnly == false)
        {
            query = query.Where(r => !r.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.RoleCode.ToLower().Contains(term)
                || r.RoleName.ToLower().Contains(term)
                || (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);

        query = sortBy switch
        {
            "rolecode" => input.SortDesc
                ? query.OrderByDescending(r => r.RoleCode).ThenBy(r => r.IdRole)
                : query.OrderBy(r => r.RoleCode).ThenBy(r => r.IdRole),
            "usercount" => input.SortDesc
                ? query.OrderByDescending(r => _db.UserRoles.Count(ur => ur.FkRole == r.IdRole && !ur.User!.Cancelled))
                    .ThenBy(r => r.RoleName).ThenBy(r => r.IdRole)
                : query.OrderBy(r => _db.UserRoles.Count(ur => ur.FkRole == r.IdRole && !ur.User!.Cancelled))
                    .ThenBy(r => r.RoleName).ThenBy(r => r.IdRole),
            "isactive" => input.SortDesc
                ? query.OrderByDescending(r => r.IsActive).ThenBy(r => r.RoleName).ThenBy(r => r.IdRole)
                : query.OrderBy(r => r.IsActive).ThenBy(r => r.RoleName).ThenBy(r => r.IdRole),
            _ => input.SortDesc
                ? query.OrderByDescending(r => r.RoleName).ThenBy(r => r.IdRole)
                : query.OrderBy(r => r.RoleName).ThenBy(r => r.IdRole)
        };

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new OutputRoleSelect
            {
                IdRole = r.IdRole,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                Description = r.Description,
                IsSystem = r.IsSystem,
                IsActive = r.IsActive,
                UserCount = _db.UserRoles.Count(ur => ur.FkRole == r.IdRole && !ur.User!.Cancelled),
                PermissionCount = _db.RolePermissions.Count(rp =>
                    rp.FkRole == r.IdRole && rp.Permission != null && rp.Permission.IsActive && !rp.Permission.Cancelled)
            })
            .ToListAsync(ct);

        return new OutputRoleList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Roles = rows
        };
    }

    public async Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default)
    {
        var role = await _db.Roles.AsNoTracking()
            .Where(r => r.IdRole == input.IdRole && !r.Cancelled)
            .Select(r => new OutputRoleSelect
            {
                IdRole = r.IdRole,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                Description = r.Description,
                IsSystem = r.IsSystem,
                IsActive = r.IsActive,
                UserCount = _db.UserRoles.Count(ur => ur.FkRole == r.IdRole && !ur.User!.Cancelled)
            })
            .FirstOrDefaultAsync(ct);

        if (role is null) return null;

        role.PermissionIds = await _db.RolePermissions.AsNoTracking()
            .Where(rp => rp.FkRole == role.IdRole
                         && rp.Permission != null && rp.Permission.IsActive && !rp.Permission.Cancelled)
            .Select(rp => rp.FkPermission)
            .ToListAsync(ct);
        role.PermissionCount = role.PermissionIds.Count;

        return role;
    }

    public async Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var entity = new Role
        {
            RoleCode = input.RoleCode.Trim().ToUpperInvariant(),
            RoleName = input.RoleName.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            IsSystem = false,
            IsActive = input.IsActive,
            CreatedAt = now
        };
        _db.Roles.Add(entity);

        foreach (var permissionId in input.PermissionIds.Distinct())
        {
            _db.RolePermissions.Add(new RolePermission
            {
                Role = entity,
                FkPermission = permissionId,
                CreatedAt = now
            });
        }

        // One SaveChanges = one transaction: the role never exists without its permissions.
        await _db.SaveChangesAsync(ct);

        return new OutputRoleCreate { IdRole = entity.IdRole, RoleCode = entity.RoleCode };
    }

    public async Task<OutputRoleUpdate?> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Roles.FirstOrDefaultAsync(r => r.IdRole == input.IdRole && !r.Cancelled, ct);
        if (entity is null) return null;

        var now = DateTime.UtcNow;
        entity.RoleName = input.RoleName.Trim();
        entity.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        entity.UpdatedAt = now;

        // Apply only the difference. Rows that point at permissions which are no longer active
        // are left untouched so they come back if the permission is re-enabled.
        var desired = input.PermissionIds.Distinct().ToHashSet();
        var activePermissionIds = await _db.Permissions
            .Where(p => p.IsActive && !p.Cancelled)
            .Select(p => p.IdPermission)
            .ToListAsync(ct);
        var activeSet = activePermissionIds.ToHashSet();

        var existing = await _db.RolePermissions.Where(rp => rp.FkRole == entity.IdRole).ToListAsync(ct);

        var toRemove = existing
            .Where(rp => activeSet.Contains(rp.FkPermission) && !desired.Contains(rp.FkPermission))
            .ToList();
        if (toRemove.Count > 0)
        {
            _db.RolePermissions.RemoveRange(toRemove);
        }

        var existingIds = existing.Select(rp => rp.FkPermission).ToHashSet();
        foreach (var permissionId in desired.Where(id => !existingIds.Contains(id)))
        {
            _db.RolePermissions.Add(new RolePermission
            {
                FkRole = entity.IdRole,
                FkPermission = permissionId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);

        return new OutputRoleUpdate { IdRole = entity.IdRole };
    }

    public async Task<OutputRoleStatusUpdate?> UpdateRoleStatusAsync(InputRoleStatusUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Roles.FirstOrDefaultAsync(r => r.IdRole == input.IdRole && !r.Cancelled, ct);
        if (entity is null) return null;

        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputRoleStatusUpdate { IdRole = entity.IdRole, IsActive = entity.IsActive };
    }

    public async Task<OutputRoleDelete?> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default)
    {
        var entity = await _db.Roles.FirstOrDefaultAsync(r => r.IdRole == input.IdRole && !r.Cancelled, ct);
        if (entity is null) return null;

        var now = DateTime.UtcNow;
        entity.Cancelled = true;
        entity.CancelledOn = now;
        entity.CancelledReason = $"Deleted by user {input.ActorUserId}.";
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        return new OutputRoleDelete { IdRole = entity.IdRole };
    }

    public Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default) =>
        _db.Permissions.AsNoTracking()
            .Where(p => p.IsActive && !p.Cancelled)
            .Join(_db.Modules.Where(m => m.IsActive && !m.Cancelled),
                p => p.FkModule,
                m => m.IdModule,
                (p, m) => new OutputPermissionSelect
                {
                    IdPermission = p.IdPermission,
                    PermissionCode = p.PermissionCode,
                    PermissionName = p.PermissionName,
                    FkModule = p.FkModule,
                    ModuleCode = m.ModuleCode,
                    ModuleName = m.ModuleName,
                    ModuleDisplayOrder = m.DisplayOrder,
                    DisplayOrder = p.DisplayOrder
                })
            .OrderBy(p => p.ModuleDisplayOrder)
            .ThenBy(p => p.ModuleName)
            .ThenBy(p => p.DisplayOrder)
            .ThenBy(p => p.PermissionCode)
            .ToListAsync(ct);

    public Task<bool> IsRoleCodeTakenAsync(string roleCode, CancellationToken ct = default)
    {
        var code = roleCode.Trim().ToUpperInvariant();
        return _db.Roles.AsNoTracking().AnyAsync(r => r.RoleCode.ToUpper() == code, ct);
    }

    public Task<bool> IsRoleNameTakenAsync(string roleName, int? excludeIdRole, CancellationToken ct = default)
    {
        var name = roleName.Trim().ToLowerInvariant();
        var excludeId = excludeIdRole ?? 0;
        return _db.Roles.AsNoTracking().AnyAsync(
            r => !r.Cancelled && r.IdRole != excludeId && r.RoleName.ToLower() == name, ct);
    }
}

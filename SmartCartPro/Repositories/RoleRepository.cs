using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly SmartCartDbContext _db;

    public RoleRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => !r.Cancelled)
            .OrderBy(r => r.RoleName)
            .Select(r => new
            {
                r.IdRole,
                r.RoleCode,
                r.RoleName,
                r.Description,
                r.IsSystem,
                r.IsActive,
                UserCount = _db.UserRoles.Count(ur => ur.FkRole == r.IdRole)
            })
            .ToListAsync(ct);

        return new OutputRoleList
        {
            Roles = roles.Select(r => new OutputRoleSelect
            {
                IdRole = r.IdRole,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                Description = r.Description,
                IsSystem = r.IsSystem,
                IsActive = r.IsActive,
                UserCount = r.UserCount
            }).ToList()
        };
    }

    public async Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default)
    {
        var r = await _db.Roles.AsNoTracking()
            .Where(x => x.IdRole == input.IdRole && !x.Cancelled)
            .Select(x => new
            {
                x.IdRole,
                x.RoleCode,
                x.RoleName,
                x.Description,
                x.IsSystem,
                x.IsActive,
                UserCount = _db.UserRoles.Count(ur => ur.FkRole == x.IdRole)
            })
            .FirstOrDefaultAsync(ct);

        if (r is null) return null;

        var permIds = await GetRolePermissionIdsAsync(r.IdRole, ct);

        return new OutputRoleSelect
        {
            IdRole = r.IdRole,
            RoleCode = r.RoleCode,
            RoleName = r.RoleName,
            Description = r.Description,
            IsSystem = r.IsSystem,
            IsActive = r.IsActive,
            UserCount = r.UserCount,
            PermissionIds = permIds
        };
    }

    public async Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default)
    {
        var entity = new Role
        {
            RoleCode = input.RoleCode.Trim().ToUpperInvariant(),
            RoleName = input.RoleName.Trim(),
            Description = input.Description?.Trim(),
            IsSystem = false,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Roles.Add(entity);
        await _db.SaveChangesAsync(ct);

        if (input.PermissionIds.Any())
        {
            await SetRolePermissionsAsync(entity.IdRole, input.PermissionIds, ct);
        }

        return new OutputRoleCreate
        {
            ResponseCode = 0,
            ResponseMsg = "Created",
            StatusCode = 200,
            IdRole = entity.IdRole
        };
    }

    public async Task<OutputRoleUpdate> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Roles.FirstOrDefaultAsync(
            r => r.IdRole == input.IdRole && !r.Cancelled, ct);

        if (entity is null)
        {
            return new OutputRoleUpdate { ResponseCode = 1, ResponseMsg = "Role not found.", StatusCode = 404 };
        }

        entity.RoleName = input.RoleName.Trim();
        entity.Description = input.Description?.Trim();
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await SetRolePermissionsAsync(entity.IdRole, input.PermissionIds, ct);

        return new OutputRoleUpdate { ResponseCode = 0, ResponseMsg = "Updated", StatusCode = 200 };
    }

    public async Task<OutputRoleDelete> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default)
    {
        if (await IsRoleInUseAsync(input.IdRole, ct))
        {
            return new OutputRoleDelete
            {
                ResponseCode = 1,
                ResponseMsg = "Cannot delete role — it is assigned to one or more users.",
                StatusCode = 409
            };
        }

        var entity = await _db.Roles.FirstOrDefaultAsync(
            r => r.IdRole == input.IdRole && !r.Cancelled && !r.IsSystem, ct);

        if (entity is null)
        {
            return new OutputRoleDelete
            {
                ResponseCode = 1,
                ResponseMsg = "Role not found or is a system role.",
                StatusCode = 404
            };
        }

        // Remove permissions first
        var perms = await _db.RolePermissions.Where(rp => rp.FkRole == input.IdRole).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(perms);

        entity.Cancelled = true;
        entity.CancelledOn = DateTime.UtcNow;
        entity.CancelledReason = $"Deleted by user {input.ActorUserId}";
        await _db.SaveChangesAsync(ct);

        return new OutputRoleDelete { ResponseCode = 0, ResponseMsg = "Deleted", StatusCode = 200 };
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
                    DisplayOrder = p.DisplayOrder
                })
            .OrderBy(p => p.ModuleCode)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<int>> GetRolePermissionIdsAsync(int idRole, CancellationToken ct = default) =>
        _db.RolePermissions.AsNoTracking()
            .Where(rp => rp.FkRole == idRole)
            .Select(rp => rp.FkPermission)
            .ToListAsync(ct);

    public async Task SetRolePermissionsAsync(int idRole, List<int> permissionIds, CancellationToken ct = default)
    {
        var existing = await _db.RolePermissions
            .Where(rp => rp.FkRole == idRole)
            .ToListAsync(ct);

        _db.RolePermissions.RemoveRange(existing);

        var now = DateTime.UtcNow;
        foreach (var permId in permissionIds.Distinct())
        {
            _db.RolePermissions.Add(new RolePermission
            {
                FkRole = idRole,
                FkPermission = permId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> IsRoleInUseAsync(int idRole, CancellationToken ct = default) =>
        _db.UserRoles.AnyAsync(ur => ur.FkRole == idRole, ct);
}

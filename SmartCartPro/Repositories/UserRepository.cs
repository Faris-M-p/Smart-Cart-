using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SmartCartDbContext _db;

    public UserRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default)
    {
        var query = _db.Users.AsNoTracking()
            .Where(u => !u.Cancelled && !u.IsPlatformSuperAdmin);

        if (input.ActiveOnly)
        {
            query = query.Where(u => u.IsActive);
        }

        // Load users with their first role via UserRoles join
        var users = await query
            .OrderBy(u => u.FullName)
            .Select(u => new
            {
                u.IdUser,
                u.UserName,
                u.Email,
                u.FullName,
                u.Phone,
                u.EmployeeCode,
                u.IsActive,
                u.MustChangePassword,
                u.LastLoginAt,
                Role = u.UserRoles
                    .Where(ur => ur.Role != null && !ur.Role.Cancelled)
                    .Select(ur => new { ur.Role!.IdRole, ur.Role.RoleName })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var storeAccess = await _db.UserStoreAccess.AsNoTracking()
            .Where(a => !a.Cancelled)
            .Select(a => new { a.FkUser, a.FkStore })
            .ToListAsync(ct);

        var userIds = users.Select(u => u.IdUser).ToHashSet();
        var accessByUser = storeAccess
            .Where(a => userIds.Contains(a.FkUser))
            .GroupBy(a => a.FkUser)
            .ToDictionary(g => g.Key, g => g.Select(a => a.FkStore).ToList());

        return new OutputUserList
        {
            Users = users.Select(u => new OutputUserSelect
            {
                IdUser = u.IdUser,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                EmployeeCode = u.EmployeeCode,
                IsActive = u.IsActive,
                MustChangePassword = u.MustChangePassword,
                LastLoginAt = u.LastLoginAt,
                CurrentRoleId = u.Role?.IdRole,
                CurrentRoleName = u.Role?.RoleName,
                StoreIds = accessByUser.TryGetValue(u.IdUser, out var stores) ? stores : new List<int>()
            }).ToList()
        };
    }

    public async Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default)
    {
        var u = await _db.Users.AsNoTracking()
            .Where(x => x.IdUser == input.IdUser && !x.Cancelled && !x.IsPlatformSuperAdmin)
            .Select(x => new
            {
                x.IdUser,
                x.UserName,
                x.Email,
                x.FullName,
                x.Phone,
                x.EmployeeCode,
                x.IsActive,
                x.MustChangePassword,
                x.LastLoginAt,
                Role = x.UserRoles
                    .Where(ur => ur.Role != null && !ur.Role.Cancelled)
                    .Select(ur => new { ur.Role!.IdRole, ur.Role.RoleName })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (u is null) return null;

        var storeIds = await _db.UserStoreAccess.AsNoTracking()
            .Where(a => a.FkUser == input.IdUser && !a.Cancelled)
            .Select(a => a.FkStore)
            .ToListAsync(ct);

        return new OutputUserSelect
        {
            IdUser = u.IdUser,
            UserName = u.UserName,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            EmployeeCode = u.EmployeeCode,
            IsActive = u.IsActive,
            MustChangePassword = u.MustChangePassword,
            LastLoginAt = u.LastLoginAt,
            CurrentRoleId = u.Role?.IdRole,
            CurrentRoleName = u.Role?.RoleName,
            StoreIds = storeIds
        };
    }

    public async Task<OutputUserCreate> CreateUserAsync(InputUserCreate input, CancellationToken ct = default)
    {
        var entity = new User
        {
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim().ToLowerInvariant(),
            PasswordHash = input.PasswordHash,
            FullName = input.FullName.Trim(),
            Phone = input.Phone?.Trim(),
            EmployeeCode = input.EmployeeCode?.Trim(),
            IsActive = true,
            MustChangePassword = true,
            IsPlatformSuperAdmin = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync(ct);

        // Assign role
        if (input.FkRole.HasValue)
        {
            _db.UserRoles.Add(new UserRole
            {
                FkUser = entity.IdUser,
                FkRole = input.FkRole.Value,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Assign stores
        foreach (var storeId in input.StoreIds.Distinct())
        {
            _db.UserStoreAccess.Add(new UserStoreAccess
            {
                FkUser = entity.IdUser,
                FkStore = storeId,
                IsDefault = input.StoreIds.IndexOf(storeId) == 0,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (input.FkRole.HasValue || input.StoreIds.Any())
        {
            await _db.SaveChangesAsync(ct);
        }

        return new OutputUserCreate
        {
            ResponseCode = 0,
            ResponseMsg = "Created",
            StatusCode = 200,
            IdUser = entity.IdUser,
            UserName = entity.UserName
        };
    }

    public async Task<OutputUserUpdate> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(
            u => u.IdUser == input.IdUser && !u.Cancelled && !u.IsPlatformSuperAdmin, ct);

        if (entity is null)
        {
            return new OutputUserUpdate { ResponseCode = 1, ResponseMsg = "User not found.", StatusCode = 404 };
        }

        entity.FullName = input.FullName.Trim();
        entity.Phone = input.Phone?.Trim();
        entity.EmployeeCode = input.EmployeeCode?.Trim();
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new OutputUserUpdate { ResponseCode = 0, ResponseMsg = "Updated", StatusCode = 200 };
    }

    public async Task<bool> DeactivateUserAsync(int idUser, CancellationToken ct = default)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(
            u => u.IdUser == idUser && !u.Cancelled && !u.IsPlatformSuperAdmin, ct);

        if (entity is null) return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ResetPasswordHashAsync(int idUser, string newPasswordHash, CancellationToken ct = default)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(
            u => u.IdUser == idUser && !u.Cancelled && !u.IsPlatformSuperAdmin, ct);

        if (entity is null) return false;

        entity.PasswordHash = newPasswordHash;
        entity.MustChangePassword = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AssignStoresToUserAsync(int idUser, List<int> storeIds, CancellationToken ct = default)
    {
        var existing = await _db.UserStoreAccess
            .Where(a => a.FkUser == idUser && !a.Cancelled)
            .ToListAsync(ct);

        _db.UserStoreAccess.RemoveRange(existing);

        var now = DateTime.UtcNow;
        var isFirst = true;
        foreach (var storeId in storeIds.Distinct())
        {
            _db.UserStoreAccess.Add(new UserStoreAccess
            {
                FkUser = idUser,
                FkStore = storeId,
                IsDefault = isFirst,
                CreatedAt = now
            });
            isFirst = false;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(int idUser, int idRole, CancellationToken ct = default)
    {
        var existing = await _db.UserRoles
            .Where(ur => ur.FkUser == idUser)
            .ToListAsync(ct);

        _db.UserRoles.RemoveRange(existing);

        _db.UserRoles.Add(new UserRole
        {
            FkUser = idUser,
            FkRole = idRole,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<List<OutputPermissionSelect>> GetUserPermissionsAsync(int idUser, CancellationToken ct = default) =>
        _db.UserRoles.AsNoTracking()
            .Where(ur => ur.FkUser == idUser)
            .SelectMany(ur => _db.RolePermissions
                .Where(rp => rp.FkRole == ur.FkRole)
                .Join(_db.Permissions.Where(p => p.IsActive && !p.Cancelled),
                    rp => rp.FkPermission,
                    p => p.IdPermission,
                    (rp, p) => new OutputPermissionSelect
                    {
                        IdPermission = p.IdPermission,
                        PermissionCode = p.PermissionCode,
                        PermissionName = p.PermissionName,
                        FkModule = p.FkModule,
                        DisplayOrder = p.DisplayOrder
                    }))
            .Distinct()
            .ToListAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Employees (users), their single role and their store access. EF/LINQ only.
/// Platform super admin accounts are excluded from every read and write here, and a company actor only
/// reaches employees that have access to a store of that company.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly SmartCartDbContext _db;

    public UserRepository(SmartCartDbContext db) => _db = db;

    /// <summary>True when the actor is limited to one company (anything except null or 0 = platform super admin).</summary>
    private static bool IsCompanyScoped(int? actorFkCompany) =>
        actorFkCompany.HasValue && actorFkCompany.Value != 0;

    /// <summary>
    /// Employees the actor may manage: not cancelled, never a platform super admin and, for a company actor,
    /// only people with an active access row on a store of that company.
    /// </summary>
    private IQueryable<User> ScopedUsers(int? actorFkCompany, bool track)
    {
        var query = track ? _db.Users.AsQueryable() : _db.Users.AsNoTracking();
        query = query.Where(u => !u.Cancelled && !u.IsPlatformSuperAdmin);

        if (IsCompanyScoped(actorFkCompany))
        {
            var companyId = actorFkCompany!.Value;
            query = query.Where(u => u.StoreAccess.Any(a => !a.Cancelled && a.Store!.FkCompany == companyId));
        }

        return query;
    }

    public async Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = input.PageSize <= 0 ? 25 : Math.Min(input.PageSize, 100);
        var sortBy = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();

        var query = ScopedUsers(input.ActorFkCompany, track: false);

        if (input.ActiveOnly == true)
        {
            query = query.Where(u => u.IsActive);
        }
        else if (input.ActiveOnly == false)
        {
            query = query.Where(u => !u.IsActive);
        }

        if (input.FkRole.HasValue)
        {
            var roleId = input.FkRole.Value;
            query = query.Where(u => u.UserRoles.Any(ur => ur.FkRole == roleId));
        }

        if (input.FkStore.HasValue)
        {
            var storeId = input.FkStore.Value;
            query = query.Where(u => u.StoreAccess.Any(a => a.FkStore == storeId && !a.Cancelled));
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term)
                || u.UserName.ToLower().Contains(term)
                || u.Email.ToLower().Contains(term)
                || (u.EmployeeCode != null && u.EmployeeCode.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);

        query = sortBy switch
        {
            "username" => input.SortDesc
                ? query.OrderByDescending(u => u.UserName).ThenBy(u => u.IdUser)
                : query.OrderBy(u => u.UserName).ThenBy(u => u.IdUser),
            "email" => input.SortDesc
                ? query.OrderByDescending(u => u.Email).ThenBy(u => u.IdUser)
                : query.OrderBy(u => u.Email).ThenBy(u => u.IdUser),
            "role" => input.SortDesc
                ? query.OrderByDescending(u => u.UserRoles.Min(ur => ur.Role!.RoleName)).ThenBy(u => u.FullName).ThenBy(u => u.IdUser)
                : query.OrderBy(u => u.UserRoles.Min(ur => ur.Role!.RoleName)).ThenBy(u => u.FullName).ThenBy(u => u.IdUser),
            "lastlogin" => input.SortDesc
                ? query.OrderByDescending(u => u.LastLoginAt).ThenBy(u => u.IdUser)
                : query.OrderBy(u => u.LastLoginAt).ThenBy(u => u.IdUser),
            "isactive" => input.SortDesc
                ? query.OrderByDescending(u => u.IsActive).ThenBy(u => u.FullName).ThenBy(u => u.IdUser)
                : query.OrderBy(u => u.IsActive).ThenBy(u => u.FullName).ThenBy(u => u.IdUser),
            _ => input.SortDesc
                ? query.OrderByDescending(u => u.FullName).ThenBy(u => u.IdUser)
                : query.OrderBy(u => u.FullName).ThenBy(u => u.IdUser)
        };

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                    .OrderBy(ur => ur.IdUserRole)
                    .Select(ur => new { ur.Role!.IdRole, ur.Role.RoleName })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var accessByUser = await LoadStoreAccessAsync(rows.Select(r => r.IdUser).ToList(), input.ActorFkCompany, ct);

        return new OutputUserList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Users = rows.Select(u => new OutputUserSelect
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
                Stores = accessByUser.TryGetValue(u.IdUser, out var stores) ? stores : new List<OutputUserStoreAccess>()
            }).ToList()
        };
    }

    public async Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default)
    {
        var u = await ScopedUsers(input.ActorFkCompany, track: false)
            .Where(x => x.IdUser == input.IdUser)
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
                    .OrderBy(ur => ur.IdUserRole)
                    .Select(ur => new { ur.Role!.IdRole, ur.Role.RoleName })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (u is null) return null;

        var accessByUser = await LoadStoreAccessAsync(new List<int> { u.IdUser }, input.ActorFkCompany, ct);

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
            Stores = accessByUser.TryGetValue(u.IdUser, out var stores) ? stores : new List<OutputUserStoreAccess>()
        };
    }

    public async Task<OutputUserOptionList> ListUserOptionsAsync(InputUserOptionList input, CancellationToken ct = default)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => !r.Cancelled)
            .OrderBy(r => r.RoleName)
            .Select(r => new OutputUserRoleSelect
            {
                IdRole = r.IdRole,
                RoleCode = r.RoleCode,
                RoleName = r.RoleName,
                IsActive = r.IsActive
            })
            .ToListAsync(ct);

        var storeQuery = _db.Stores.AsNoTracking().Where(s => !s.Cancelled);
        if (IsCompanyScoped(input.FkCompany))
        {
            var companyId = input.FkCompany!.Value;
            storeQuery = storeQuery.Where(s => s.FkCompany == companyId);
        }

        var stores = await storeQuery
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.StoreName)
            .Select(s => new OutputUserStoreSelect
            {
                IdStore = s.IdStore,
                StoreCode = s.StoreCode,
                StoreName = s.StoreName,
                IsActive = s.IsActive
            })
            .ToListAsync(ct);

        return new OutputUserOptionList { Roles = roles, Stores = stores };
    }

    public Task<bool> IsLoginIdentifierTakenAsync(string identifier, int? excludeIdUser, CancellationToken ct = default)
    {
        var value = identifier.Trim().ToLowerInvariant();
        var excludeId = excludeIdUser ?? 0;

        return _db.Users.AsNoTracking().AnyAsync(
            u => u.IdUser != excludeId
                 && (u.UserName.ToLower() == value || u.Email.ToLower() == value),
            ct);
    }

    public async Task<OutputUserCreate> CreateUserAsync(InputUserCreate input, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var entity = new User
        {
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim().ToLowerInvariant(),
            PasswordHash = input.PasswordHash,
            FullName = input.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
            EmployeeCode = string.IsNullOrWhiteSpace(input.EmployeeCode) ? null : input.EmployeeCode.Trim(),
            IsActive = true,
            MustChangePassword = input.MustChangePassword,
            IsPlatformSuperAdmin = false,
            CreatedAt = now
        };

        entity.UserRoles.Add(new UserRole { FkRole = input.FkRole, CreatedAt = now });

        foreach (var storeId in input.StoreIds.Distinct())
        {
            entity.StoreAccess.Add(new UserStoreAccess
            {
                FkStore = storeId,
                IsDefault = input.DefaultStoreId.HasValue && storeId == input.DefaultStoreId.Value,
                CreatedAt = now
            });
        }

        // One SaveChanges = one transaction: the employee never exists without role and stores.
        _db.Users.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new OutputUserCreate { IdUser = entity.IdUser, UserName = entity.UserName };
    }

    public async Task<OutputUserUpdate?> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = await ScopedUsers(input.ActorFkCompany, track: true)
            .FirstOrDefaultAsync(u => u.IdUser == input.IdUser, ct);
        if (entity is null) return null;

        var now = DateTime.UtcNow;

        entity.Email = input.Email.Trim().ToLowerInvariant();
        entity.FullName = input.FullName.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        entity.EmployeeCode = string.IsNullOrWhiteSpace(input.EmployeeCode) ? null : input.EmployeeCode.Trim();
        entity.UpdatedAt = now;

        // Role: an employee holds exactly one role.
        var roles = await _db.UserRoles.Where(ur => ur.FkUser == entity.IdUser).ToListAsync(ct);
        var staleRoles = roles.Where(r => r.FkRole != input.FkRole).ToList();
        if (staleRoles.Count > 0)
        {
            _db.UserRoles.RemoveRange(staleRoles);
        }

        if (roles.All(r => r.FkRole != input.FkRole))
        {
            _db.UserRoles.Add(new UserRole { FkUser = entity.IdUser, FkRole = input.FkRole, CreatedAt = now });
        }

        // Store access (soft cancel). Only the stores of the actor's company are managed here: rows for stores of
        // another company (an employee shared across companies) are left exactly as they are.
        // Pass 1 cancels/revives/adds rows and clears stale default flags, pass 2 sets the single default,
        // so the "one default per user" index is never violated.
        HashSet<int>? managedStoreIds = null;
        if (IsCompanyScoped(input.ActorFkCompany))
        {
            var companyId = input.ActorFkCompany!.Value;
            var companyStoreIds = await _db.Stores.AsNoTracking()
                .Where(s => s.FkCompany == companyId)
                .Select(s => s.IdStore)
                .ToListAsync(ct);
            managedStoreIds = new HashSet<int>(companyStoreIds);
        }

        bool IsManaged(int storeId) => managedStoreIds is null || managedStoreIds.Contains(storeId);

        var wanted = input.StoreIds.Distinct().Where(IsManaged).ToList();
        var access = await _db.UserStoreAccess.Where(a => a.FkUser == entity.IdUser).ToListAsync(ct);

        // A default held on another company's store cannot be moved from here (one default per employee).
        var defaultStoreId = access.Any(a => !a.Cancelled && a.IsDefault && !IsManaged(a.FkStore))
            ? 0
            : (input.DefaultStoreId.HasValue && wanted.Contains(input.DefaultStoreId.Value)
                ? input.DefaultStoreId.Value
                : (wanted.Count > 0 ? wanted[0] : 0));

        foreach (var row in access.Where(a => IsManaged(a.FkStore)))
        {
            var keep = wanted.Contains(row.FkStore);
            var wasCancelled = row.Cancelled;

            if (keep && wasCancelled)
            {
                row.Cancelled = false;
                row.CancelledOn = null;
                row.CancelledReason = null;
                row.UpdatedAt = now;
            }
            else if (!keep && !wasCancelled)
            {
                row.Cancelled = true;
                row.CancelledOn = now;
                row.CancelledReason = "Removed from the employee's store access.";
                row.UpdatedAt = now;
            }

            var keepsDefault = keep && !wasCancelled && row.FkStore == defaultStoreId;
            if (row.IsDefault && !keepsDefault)
            {
                row.IsDefault = false;
                row.UpdatedAt = now;
            }
        }

        var added = new List<UserStoreAccess>();
        foreach (var storeId in wanted.Where(id => access.All(a => a.FkStore != id)))
        {
            var row = new UserStoreAccess
            {
                FkUser = entity.IdUser,
                FkStore = storeId,
                IsDefault = false,
                CreatedAt = now
            };
            added.Add(row);
            _db.UserStoreAccess.Add(row);
        }

        await _db.SaveChangesAsync(ct);

        var defaultRow = access.Concat(added)
            .FirstOrDefault(a => a.FkStore == defaultStoreId && !a.Cancelled);
        if (defaultRow is not null && !defaultRow.IsDefault)
        {
            defaultRow.IsDefault = true;
            defaultRow.UpdatedAt = now;
            await _db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        return new OutputUserUpdate { IdUser = entity.IdUser };
    }

    public async Task<OutputUserStatusUpdate?> UpdateUserStatusAsync(InputUserStatusUpdate input, CancellationToken ct = default)
    {
        var entity = await ScopedUsers(input.ActorFkCompany, track: true)
            .FirstOrDefaultAsync(u => u.IdUser == input.IdUser, ct);
        if (entity is null) return null;

        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputUserStatusUpdate { IdUser = entity.IdUser, IsActive = entity.IsActive };
    }

    public async Task<OutputUserPasswordUpdate?> UpdateUserPasswordAsync(InputUserPasswordUpdate input, CancellationToken ct = default)
    {
        var entity = await ScopedUsers(input.ActorFkCompany, track: true)
            .FirstOrDefaultAsync(u => u.IdUser == input.IdUser, ct);
        if (entity is null) return null;

        entity.PasswordHash = input.PasswordHash;
        entity.MustChangePassword = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputUserPasswordUpdate { IdUser = entity.IdUser };
    }

    // Used by shared filters and the layout (ICurrentUserAccess). Keep as is.
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

    /// <summary>
    /// Active (not cancelled) store access per user, default store first.
    /// A company actor only sees the stores of that company.
    /// </summary>
    private async Task<Dictionary<int, List<OutputUserStoreAccess>>> LoadStoreAccessAsync(
        List<int> userIds, int? actorFkCompany, CancellationToken ct)
    {
        var result = new Dictionary<int, List<OutputUserStoreAccess>>();
        if (userIds.Count == 0) return result;

        var storeQuery = _db.Stores.AsNoTracking().Where(s => !s.Cancelled);
        if (IsCompanyScoped(actorFkCompany))
        {
            var companyId = actorFkCompany!.Value;
            storeQuery = storeQuery.Where(s => s.FkCompany == companyId);
        }

        var rows = await (
            from a in _db.UserStoreAccess.AsNoTracking()
            join s in storeQuery on a.FkStore equals s.IdStore
            where userIds.Contains(a.FkUser) && !a.Cancelled
            orderby a.IsDefault descending, s.StoreName
            select new { a.FkUser, s.IdStore, s.StoreName, a.IsDefault, s.IsActive })
            .ToListAsync(ct);

        foreach (var group in rows.GroupBy(r => r.FkUser))
        {
            result[group.Key] = group
                .Select(r => new OutputUserStoreAccess
                {
                    IdStore = r.IdStore,
                    StoreName = r.StoreName,
                    IsDefault = r.IsDefault,
                    IsActive = r.IsActive
                })
                .ToList();
        }

        return result;
    }
}

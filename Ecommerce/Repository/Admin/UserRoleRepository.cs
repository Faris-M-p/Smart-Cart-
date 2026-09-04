using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.UserRoles;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.UserRoleModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class UserRoleRepository : IUserRoleInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public UserRoleRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserRole?> GetUserRoleByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var role = await _dbContext.UserRoles
                .AsNoTracking()
                .Where(r => r.ID_UserRole == id)
                .Select(r => new UserRole
                {
                    UserRoleID = r.ID_UserRole,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsSystemRole = r.IsSystemRole,
                    IsActive = r.IsActive,
                    Cancelled = r.Cancelled,
                    CancelledOn = r.CancelledOn,
                    CancelledReason = r.CancelledReason
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return null;
            }

            role.SelectedPermissionIds = await _dbContext.UserRolePermissions
                .AsNoTracking()
                .Where(p => p.FK_UserRole == id && !p.Cancelled)
                .Select(p => p.FK_Permission)
                .ToListAsync();

            return role;
        }

        public async Task<TableOutput<UserRole>> GetUserRoleListAsync(UserRoleListInput input)
        {
            if (input == null)
            {
                return UserRoleHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = UserRoleHelper.NormalizeInput(input);

                var filteredQuery = UserRoleHelper.ApplyFilters(
                    _dbContext.UserRoles.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = UserRoleHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pagedEntityQuery = sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var rows = await pagedEntityQuery
                    .Select(r => new UserRole
                    {
                        UserRoleID = r.ID_UserRole,
                        RoleName = r.RoleName,
                        Description = r.Description,
                        IsSystemRole = r.IsSystemRole,
                        IsActive = r.IsActive,
                        Cancelled = r.Cancelled,
                        CancelledOn = r.CancelledOn,
                        CancelledReason = r.CancelledReason
                    })
                    .ToListAsync();

                return new TableOutput<UserRole>
                {
                    TableData = rows,
                    TableSettings = new TableOutput_Settings
                    {
                        PageIndex = normalized.PageIndex,
                        PageSize = normalized.PageSize,
                        TotalCount = totalCount
                    }
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ModulePermissionNode>> GetPermissionTreeAsync()
        {
            var rows = await (
                from m in _dbContext.Modules.AsNoTracking()
                join p in _dbContext.Permissions.AsNoTracking() on m.ID_Module equals p.FK_Module
                where !m.Cancelled && m.IsActive && !p.Cancelled && p.IsActive
                orderby m.DisplayOrder, p.DisplayOrder, p.PermissionName
                select new
                {
                    m.ID_Module,
                    m.ModuleName,
                    m.DisplayName,
                    ModuleDisplayOrder = m.DisplayOrder,
                    p.ID_Permission,
                    p.PermissionName,
                    p.PermissionCode,
                    PermissionDisplayOrder = p.DisplayOrder
                }
            ).ToListAsync();

            return rows
                .GroupBy(x => new { x.ID_Module, x.ModuleName, x.DisplayName, x.ModuleDisplayOrder })
                .OrderBy(g => g.Key.ModuleDisplayOrder)
                .Select(g => new ModulePermissionNode
                {
                    ModuleID = g.Key.ID_Module,
                    ModuleName = g.Key.ModuleName,
                    DisplayName = g.Key.DisplayName,
                    Permissions = g
                        .OrderBy(x => x.PermissionDisplayOrder)
                        .ThenBy(x => x.PermissionName)
                        .Select(x => new PermissionNode
                        {
                            PermissionID = x.ID_Permission,
                            PermissionName = x.PermissionName,
                            PermissionCode = x.PermissionCode
                        })
                        .ToList()
                })
                .ToList();
        }

        public async Task<CommonResponse> CreateUserRoleAsync(UserRoleUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Please enter role name.");
            }

            try
            {
                var normalized = UserRoleHelper.NormalizeInput(input);
                if (normalized.RoleName.Length == 0)
                {
                    return Fail("Please enter role name.");
                }

                if (await RoleNameExistsForActiveAsync(normalized.RoleName, excludeRoleId: 0))
                {
                    return Fail($"Role name \"{normalized.RoleName}\" already exists.");
                }

                var now = DateTime.Now;
                var entity = new UserRoleEntity
                {
                    RoleName = normalized.RoleName,
                    Description = normalized.Description,
                    IsSystemRole = false,
                    IsActive = normalized.IsActive,
                    CreatedAt = now,
                    Cancelled = false
                };

                _dbContext.UserRoles.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_UserRole, "User role created successfully.");
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> UpdateUserRoleAsync(UserRoleUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = UserRoleHelper.NormalizeInput(input);
                if (normalized.RoleName.Length == 0)
                {
                    return Fail("Please enter role name.");
                }

                if (input.UserRoleID <= 0)
                {
                    return Fail("Invalid User Role ID.");
                }

                var entity = await _dbContext.UserRoles
                    .FirstOrDefaultAsync(r => r.ID_UserRole == input.UserRoleID);

                if (entity == null)
                {
                    return Fail("Invalid User Role ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This user role is deleted and cannot be edited.");
                }

                if (entity.IsSystemRole)
                {
                    return Fail("System roles cannot be modified.");
                }

                if (await RoleNameExistsForActiveAsync(normalized.RoleName, excludeRoleId: input.UserRoleID))
                {
                    return Fail($"Role name \"{normalized.RoleName}\" already exists.");
                }

                entity.RoleName = normalized.RoleName;
                entity.Description = normalized.Description;
                entity.IsActive = normalized.IsActive;
                entity.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(input.UserRoleID, "User role updated successfully.");
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> DeleteUserRoleAsync(UserRoleDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid User Role ID.");
            }

            try
            {
                var userRoleId = input.UserRoleID;
                if (userRoleId <= 0)
                {
                    return Fail("Invalid User Role ID.");
                }

                var entity = await _dbContext.UserRoles
                    .FirstOrDefaultAsync(r => r.ID_UserRole == userRoleId);

                if (entity == null)
                {
                    return Fail("Invalid User Role ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This user role is already deleted.");
                }

                if (entity.IsSystemRole)
                {
                    return Fail("System roles cannot be deleted.");
                }

                if (await HasActiveAdminUsersAsync(userRoleId))
                {
                    return Fail("Cannot delete this role because employees are assigned to it.");
                }

                var now = DateTime.Now;
                entity.Cancelled = true;
                entity.CancelledOn = now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);
                entity.UpdatedAt = now;

                var mappings = await _dbContext.UserRolePermissions
                    .Where(p => p.FK_UserRole == userRoleId && !p.Cancelled)
                    .ToListAsync();

                foreach (var mapping in mappings)
                {
                    mapping.Cancelled = true;
                    mapping.CancelledOn = now;
                    mapping.CancelledReason = "Role cancelled";
                }

                await _dbContext.SaveChangesAsync();

                return Ok(userRoleId, "User role deleted successfully.");
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> SaveUserRolePermissionsAsync(UserRolePermissionSaveInput input)
        {
            if (input == null)
            {
                return Fail("Invalid User Role ID.");
            }

            var normalized = UserRoleHelper.NormalizeInput(input);
            if (normalized.UserRoleID <= 0)
            {
                return Fail("Invalid User Role ID.");
            }

            if (normalized.SelectedPermissionIds.Count == 0)
            {
                return Fail("At least one permission must be selected.");
            }

            var entity = await _dbContext.UserRoles
                .FirstOrDefaultAsync(r => r.ID_UserRole == normalized.UserRoleID);

            if (entity == null)
            {
                return Fail("Invalid User Role ID.");
            }

            if (entity.Cancelled)
            {
                return Fail("This user role is deleted and cannot be edited.");
            }

            if (entity.IsSystemRole)
            {
                return Fail("This system role cannot be modified.");
            }

            if (!await AllPermissionsExistAsync(normalized.SelectedPermissionIds))
            {
                return Fail("One or more selected permissions are invalid.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                entity.UpdatedAt = now;

                var existing = await _dbContext.UserRolePermissions
                    .Where(p => p.FK_UserRole == entity.ID_UserRole)
                    .ToListAsync();

                var remaining = normalized.SelectedPermissionIds.ToHashSet();
                foreach (var mapping in existing)
                {
                    if (remaining.Contains(mapping.FK_Permission))
                    {
                        mapping.Cancelled = false;
                        mapping.CancelledOn = null;
                        mapping.CancelledReason = null;
                        remaining.Remove(mapping.FK_Permission);
                    }
                    else if (!mapping.Cancelled)
                    {
                        mapping.Cancelled = true;
                        mapping.CancelledOn = now;
                        mapping.CancelledReason = "Replaced during permission mapping";
                    }
                }

                AddPermissionMappings(entity.ID_UserRole, remaining.ToList(), now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(entity.ID_UserRole, "Permissions saved successfully.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private void AddPermissionMappings(int roleId, IReadOnlyList<int> permissionIds, DateTime now)
        {
            foreach (var permissionId in permissionIds)
            {
                _dbContext.UserRolePermissions.Add(new UserRolePermissionEntity
                {
                    FK_UserRole = roleId,
                    FK_Permission = permissionId,
                    CreatedAt = now,
                    Cancelled = false
                });
            }
        }

        private async Task<bool> RoleNameExistsForActiveAsync(string trimmedName, int excludeRoleId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.UserRoles.AnyAsync(r =>
                !r.Cancelled &&
                r.ID_UserRole != excludeRoleId &&
                (r.RoleName ?? string.Empty).ToLower() == key);
        }

        private Task<bool> HasActiveAdminUsersAsync(int userRoleId) =>
            _dbContext.AdminUsers.AnyAsync(u =>
                u.FK_UserRole == userRoleId && u.Cancelled != true);

        private async Task<bool> AllPermissionsExistAsync(IReadOnlyList<int> permissionIds)
        {
            var count = await _dbContext.Permissions
                .Where(p => !p.Cancelled && permissionIds.Contains(p.ID_Permission))
                .CountAsync();
            return count == permissionIds.Count;
        }

        private static CommonResponse Ok(long responseCode, string message) =>
            new()
            {
                ResponseCode = responseCode,
                StatusCode = true,
                ResponseMsg = message
            };

        private static CommonResponse Fail(string message) =>
            new()
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = message
            };
    }
}

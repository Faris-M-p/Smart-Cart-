using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.Employees;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.EmployeeModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class EmployeeRepository : IEmployeeInterface
    {
        private const int MinimumPasswordLength = 6;

        private readonly EcommerceDbContext _dbContext;
        private readonly IPasswordHasher<AdminUserEntity> _passwordHasher;

        public EmployeeRepository(
            EcommerceDbContext dbContext,
            IPasswordHasher<AdminUserEntity> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<TableOutput<Employee>> GetEmployeeListAsync(EmployeeListInput input)
        {
            if (input == null)
            {
                return EmployeeHelper.EmptyTableOutput(null);
            }

            var normalized = EmployeeHelper.NormalizeInput(input);
            var filtered = EmployeeHelper.ApplyFilters(
                _dbContext.AdminUsers.AsNoTracking(),
                normalized,
                _dbContext.UserRoles.AsNoTracking());

            var totalCount = await filtered.LongCountAsync();
            var sorted = EmployeeHelper.ApplySorting(filtered, normalized.SortColumn, normalized.SortMode);

            var page = await sorted
                .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                .Take(normalized.PageSize)
                .Select(e => new
                {
                    e.ID_AdminUser,
                    e.FullName,
                    e.UserName,
                    e.FK_UserRole,
                    e.IsActive,
                    e.CreatedAt
                })
                .ToListAsync();

            var roleIds = page.Select(x => x.FK_UserRole).Distinct().ToList();
            var roleNames = await _dbContext.UserRoles.AsNoTracking()
                .Where(r => roleIds.Contains(r.ID_UserRole))
                .Select(r => new { r.ID_UserRole, r.RoleName })
                .ToListAsync();
            var roleMap = roleNames.ToDictionary(x => x.ID_UserRole, x => x.RoleName);

            var rows = page.Select(e =>
            {
                roleMap.TryGetValue(e.FK_UserRole, out var roleName);
                return new Employee
                {
                    EmployeeID = e.ID_AdminUser,
                    EmployeeName = e.FullName,
                    UserName = e.UserName,
                    FK_UserRole = e.FK_UserRole,
                    UserRoleName = roleName ?? string.Empty,
                    IsActive = e.IsActive,
                    IsProtected = EmployeeHelper.IsProtectedSystemAdmin(e.UserName),
                    CreatedAt = e.CreatedAt
                };
            }).ToList();

            return new TableOutput<Employee>
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

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var entity = await _dbContext.AdminUsers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ID_AdminUser == id);

            if (entity == null || entity.Cancelled)
            {
                return null;
            }

            var roleName = await _dbContext.UserRoles.AsNoTracking()
                .Where(r => r.ID_UserRole == entity.FK_UserRole)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync() ?? string.Empty;

            return new Employee
            {
                EmployeeID = entity.ID_AdminUser,
                EmployeeName = entity.FullName,
                UserName = entity.UserName,
                FK_UserRole = entity.FK_UserRole,
                UserRoleName = roleName,
                IsActive = entity.IsActive,
                IsProtected = EmployeeHelper.IsProtectedSystemAdmin(entity.UserName),
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<List<EmployeeRoleOption>> GetActiveUserRolesAsync()
        {
            return await _dbContext.UserRoles.AsNoTracking()
                .Where(r => !r.Cancelled && r.IsActive)
                .OrderByDescending(r => r.IsSystemRole)
                .ThenBy(r => r.RoleName)
                .Select(r => new EmployeeRoleOption
                {
                    UserRoleID = r.ID_UserRole,
                    RoleName = r.RoleName
                })
                .ToListAsync();
        }

        public async Task<CommonResponse> CreateEmployeeAsync(EmployeeUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            var normalized = EmployeeHelper.NormalizeInput(input);
            var validation = ValidateWrite(normalized, isCreate: true);
            if (validation != null)
            {
                return validation;
            }

            if (await UserNameExistsAsync(normalized.UserName, excludeEmployeeId: 0))
            {
                return Fail($"Username \"{normalized.UserName}\" already exists.");
            }

            if (!await IsAssignableRoleAsync(normalized.FK_UserRole))
            {
                return Fail("Please select a valid active user role.");
            }

            var entity = new AdminUserEntity
            {
                FullName = normalized.EmployeeName,
                UserName = normalized.UserName,
                Email = EmployeeHelper.BuildPlaceholderEmail(normalized.UserName),
                FK_UserRole = normalized.FK_UserRole,
                IsActive = normalized.IsActive,
                CreatedAt = DateTime.Now,
                Cancelled = false
            };
            entity.PasswordHash = _passwordHasher.HashPassword(entity, normalized.Password);

            _dbContext.AdminUsers.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(entity.ID_AdminUser, "Employee created successfully.");
        }

        public async Task<CommonResponse> UpdateEmployeeAsync(EmployeeUpdateInput input)
        {
            if (input == null || input.EmployeeID <= 0)
            {
                return Fail("Invalid Employee ID.");
            }

            var normalized = EmployeeHelper.NormalizeInput(input);
            var validation = ValidateWrite(normalized, isCreate: false);
            if (validation != null)
            {
                return validation;
            }

            var entity = await _dbContext.AdminUsers
                .FirstOrDefaultAsync(e => e.ID_AdminUser == input.EmployeeID);

            if (entity == null)
            {
                return Fail("Employee not found.");
            }

            if (entity.Cancelled)
            {
                return Fail("This employee is deleted and cannot be edited.");
            }

            if (EmployeeHelper.IsProtectedSystemAdmin(entity.UserName))
            {
                return Fail("The system administrator cannot be modified.");
            }

            if (await UserNameExistsAsync(normalized.UserName, excludeEmployeeId: entity.ID_AdminUser))
            {
                return Fail($"Username \"{normalized.UserName}\" already exists.");
            }

            if (!await IsAssignableRoleAsync(normalized.FK_UserRole))
            {
                return Fail("Please select a valid active user role.");
            }

            entity.FullName = normalized.EmployeeName;
            entity.UserName = normalized.UserName;
            entity.FK_UserRole = normalized.FK_UserRole;
            entity.IsActive = normalized.IsActive;
            entity.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(normalized.Password))
            {
                entity.PasswordHash = _passwordHasher.HashPassword(entity, normalized.Password);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(entity.ID_AdminUser, "Employee updated successfully.");
        }

        public async Task<CommonResponse> DeleteEmployeeAsync(EmployeeDeleteInput input)
        {
            if (input == null || input.EmployeeID <= 0)
            {
                return Fail("Invalid Employee ID.");
            }

            var entity = await _dbContext.AdminUsers
                .FirstOrDefaultAsync(e => e.ID_AdminUser == input.EmployeeID);

            if (entity == null)
            {
                return Fail("Employee not found.");
            }

            if (entity.Cancelled)
            {
                return Fail("This employee is already deleted.");
            }

            if (EmployeeHelper.IsProtectedSystemAdmin(entity.UserName))
            {
                return Fail("The system administrator cannot be deleted.");
            }

            var now = DateTime.Now;
            entity.Cancelled = true;
            entity.CancelledOn = now;
            entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);
            entity.UpdatedAt = now;
            entity.IsActive = false;

            await _dbContext.SaveChangesAsync();
            return Ok(entity.ID_AdminUser, "Employee deleted successfully.");
        }

        private CommonResponse? ValidateWrite(NormalizedEmployeeWriteInput input, bool isCreate)
        {
            if (input.EmployeeName.Length == 0)
            {
                return Fail("Please enter employee name.");
            }

            if (input.UserName.Length == 0)
            {
                return Fail("Please enter username.");
            }

            if (input.FK_UserRole <= 0)
            {
                return Fail("Please select a user role.");
            }

            if (isCreate)
            {
                if (string.IsNullOrWhiteSpace(input.Password))
                {
                    return Fail("Please enter password.");
                }

                if (input.Password.Length < MinimumPasswordLength)
                {
                    return Fail($"Password must be at least {MinimumPasswordLength} characters.");
                }

                if (input.Password != input.ConfirmPassword)
                {
                    return Fail("Password and confirm password do not match.");
                }
            }
            else if (!string.IsNullOrWhiteSpace(input.Password))
            {
                if (input.Password.Length < MinimumPasswordLength)
                {
                    return Fail($"Password must be at least {MinimumPasswordLength} characters.");
                }

                if (input.Password != input.ConfirmPassword)
                {
                    return Fail("New password and confirm password do not match.");
                }
            }

            return null;
        }

        private Task<bool> UserNameExistsAsync(string userName, int excludeEmployeeId)
        {
            var key = userName.ToLowerInvariant();
            return _dbContext.AdminUsers.AnyAsync(e =>
                e.ID_AdminUser != excludeEmployeeId &&
                (e.UserName ?? string.Empty).ToLower() == key);
        }

        private Task<bool> IsAssignableRoleAsync(int roleId) =>
            _dbContext.UserRoles.AnyAsync(r =>
                r.ID_UserRole == roleId && !r.Cancelled && r.IsActive);

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

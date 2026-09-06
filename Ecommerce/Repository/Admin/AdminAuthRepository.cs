using Ecommerce.DataAccess;
using Ecommerce.Helpers.AdminAuth;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Ecommerce.Helpers.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.AdminAuthModel;

namespace Ecommerce.Repository.Admin
{
    public class AdminAuthRepository : IAdminAuthInterface
    {
        private readonly EcommerceDbContext _dbContext;
        private readonly IPasswordHasher<AdminUserEntity> _passwordHasher;
        private readonly AdminJwtTokenService _jwtTokenService;

        public AdminAuthRepository(
            EcommerceDbContext dbContext,
            IPasswordHasher<AdminUserEntity> passwordHasher,
            AdminJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AdminAuthOperationResult> LoginAsync(AdminLoginRequest request)
        {
            var userName = AdminAuthHelper.NormalizeUserName(request.UserName);
            var password = request.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                return Failure(StatusCodes.Status401Unauthorized, AdminAuthHelper.InvalidCredentialsMessage);
            }

            var employee = await _dbContext.AdminUsers
                .FirstOrDefaultAsync(e => e.UserName.ToLower() == userName.ToLower());

            if (employee == null || employee.Cancelled)
            {
                return Failure(StatusCodes.Status401Unauthorized, AdminAuthHelper.InvalidCredentialsMessage);
            }

            if (!employee.IsActive)
            {
                return Failure(StatusCodes.Status403Forbidden, AdminAuthHelper.InactiveAccountMessage);
            }

            var verification = _passwordHasher.VerifyHashedPassword(employee, employee.PasswordHash, password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return Failure(StatusCodes.Status401Unauthorized, AdminAuthHelper.InvalidCredentialsMessage);
            }

            var role = await LoadValidRoleAsync(employee.FK_UserRole);
            if (role == null)
            {
                return Failure(StatusCodes.Status403Forbidden, AdminAuthHelper.InvalidRoleMessage);
            }

            var permissions = await LoadPermissionCodesAsync(role.ID_UserRole);
            var (token, expiresAt) = _jwtTokenService.CreateToken(
                employee.ID_AdminUser,
                employee.UserName,
                role.ID_UserRole,
                role.RoleName);

            return new AdminAuthOperationResult
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = AdminAuthHelper.LoginSuccessMessage,
                Login = new AdminLoginData
                {
                    AccessToken = token,
                    ExpiresAt = expiresAt,
                    Employee = MapEmployee(employee),
                    Role = MapRole(role),
                    Permissions = permissions
                }
            };
        }

        public async Task<AdminAuthOperationResult> GetCurrentAsync(int employeeId)
        {
            if (employeeId <= 0)
            {
                return Failure(StatusCodes.Status401Unauthorized, AdminAuthHelper.UnauthorizedMessage);
            }

            var employee = await _dbContext.AdminUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ID_AdminUser == employeeId);

            if (employee == null || employee.Cancelled)
            {
                return Failure(StatusCodes.Status401Unauthorized, AdminAuthHelper.UnauthorizedMessage);
            }

            if (!employee.IsActive)
            {
                return Failure(StatusCodes.Status403Forbidden, AdminAuthHelper.InactiveAccountMessage);
            }

            var role = await LoadValidRoleAsync(employee.FK_UserRole);
            if (role == null)
            {
                return Failure(StatusCodes.Status403Forbidden, AdminAuthHelper.InvalidRoleMessage);
            }

            var permissions = await LoadPermissionCodesAsync(role.ID_UserRole);

            return new AdminAuthOperationResult
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Session loaded.",
                Session = new AdminSessionData
                {
                    Employee = MapEmployee(employee),
                    Role = MapRole(role),
                    Permissions = permissions
                }
            };
        }

        private async Task<UserRoleEntity?> LoadValidRoleAsync(int userRoleId)
        {
            if (userRoleId <= 0)
            {
                return null;
            }

            return await _dbContext.UserRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.ID_UserRole == userRoleId
                    && r.IsActive
                    && !r.Cancelled);
        }

        private async Task<List<string>> LoadPermissionCodesAsync(int userRoleId)
        {
            return await (
                from mapping in _dbContext.UserRolePermissions.AsNoTracking()
                join permission in _dbContext.Permissions.AsNoTracking()
                    on mapping.FK_Permission equals permission.ID_Permission
                where mapping.FK_UserRole == userRoleId
                    && !mapping.Cancelled
                    && !permission.Cancelled
                    && permission.IsActive
                orderby permission.DisplayOrder
                select permission.PermissionCode
            ).ToListAsync();
        }

        private static AdminAuthEmployee MapEmployee(AdminUserEntity employee) =>
            new()
            {
                Id = employee.ID_AdminUser,
                Name = employee.FullName,
                UserName = employee.UserName
            };

        private static AdminAuthRole MapRole(UserRoleEntity role) =>
            new()
            {
                Id = role.ID_UserRole,
                Name = role.RoleName
            };

        private static AdminAuthOperationResult Failure(int statusCode, string message) =>
            new()
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
    }
}

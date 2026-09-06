using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.EmployeeModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Employees
{
    public sealed record NormalizedEmployeeListInput(
        string? SearchLower,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedEmployeeWriteInput(
        int EmployeeID,
        string EmployeeName,
        string UserName,
        string Password,
        string ConfirmPassword,
        int FK_UserRole,
        bool IsActive);

    public static class EmployeeHelper
    {
        public const string SeededAdminUserName = "admin";

        public static bool IsProtectedSystemAdmin(string? userName) =>
            string.Equals(userName?.Trim(), SeededAdminUserName, StringComparison.OrdinalIgnoreCase);

        public static NormalizedEmployeeListInput NormalizeInput(EmployeeListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var rawSearch = input.SearchText?.Trim() ?? string.Empty;

            return new NormalizedEmployeeListInput(
                rawSearch.Length >= 1 ? rawSearch.ToLowerInvariant() : null,
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedEmployeeWriteInput NormalizeInput(EmployeeUpdateInput input)
        {
            return new NormalizedEmployeeWriteInput(
                input.EmployeeID,
                (input.EmployeeName ?? string.Empty).Trim(),
                (input.UserName ?? string.Empty).Trim(),
                input.Password ?? string.Empty,
                input.ConfirmPassword ?? string.Empty,
                input.FK_UserRole,
                input.IsActive);
        }

        public static IQueryable<AdminUserEntity> ApplyFilters(
            IQueryable<AdminUserEntity> query,
            NormalizedEmployeeListInput normalized,
            IQueryable<UserRoleEntity> roles)
        {
            query = query.Where(e => e.Cancelled != true);

            if (normalized.SearchLower == null)
            {
                return query;
            }

            var s = normalized.SearchLower;
            var matchingRoleIds = roles
                .Where(r => !r.Cancelled && r.RoleName.ToLower().Contains(s))
                .Select(r => r.ID_UserRole);

            return query.Where(e =>
                e.FullName.ToLower().Contains(s)
                || e.UserName.ToLower().Contains(s)
                || matchingRoleIds.Contains(e.FK_UserRole));
        }

        public static IQueryable<AdminUserEntity> ApplySorting(
            IQueryable<AdminUserEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(EmployeeSortColumn), sortColumn))
            {
                return query.OrderByDescending(e => e.ID_AdminUser);
            }

            return (EmployeeSortColumn)sortColumn switch
            {
                EmployeeSortColumn.Name => desc
                    ? query.OrderByDescending(e => e.FullName)
                    : query.OrderBy(e => e.FullName),
                EmployeeSortColumn.UserName => desc
                    ? query.OrderByDescending(e => e.UserName)
                    : query.OrderBy(e => e.UserName),
                EmployeeSortColumn.CreatedDate => desc
                    ? query.OrderByDescending(e => e.CreatedAt)
                    : query.OrderBy(e => e.CreatedAt),
                _ => query.OrderByDescending(e => e.ID_AdminUser)
            };
        }

        public static TableOutput<Employee> EmptyTableOutput(EmployeeListInput? input)
        {
            return new TableOutput<Employee>
            {
                TableData = new List<Employee>(),
                TableSettings = new TableOutput_Settings
                {
                    PageIndex = Math.Max(1, input?.PageIndex ?? 1),
                    PageSize = Math.Max(1, input?.PageSize ?? 10),
                    TotalCount = 0
                }
            };
        }

        public static string BuildPlaceholderEmail(string userName) =>
            $"{userName.Trim().ToLowerInvariant()}@smartcart.local";
    }
}

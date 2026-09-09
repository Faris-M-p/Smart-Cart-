using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.UserRoleModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.UserRoles
{
    public sealed record NormalizedUserRoleListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedUserRoleWriteInput(
        string RoleName,
        string? Description,
        bool IsActive);

    public sealed record NormalizedUserRolePermissionInput(
        int UserRoleID,
        IReadOnlyList<int> SelectedPermissionIds);

    public static class UserRoleHelper
    {
        public static NormalizedUserRoleListInput NormalizeInput(UserRoleListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = null;
            if (rawSearch.Length >= 1)
            {
                searchLower = rawSearch.ToLowerInvariant();
            }

            return new NormalizedUserRoleListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterUserRoleIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedUserRoleWriteInput NormalizeInput(UserRoleUpdateInput input)
        {
            return new NormalizedUserRoleWriteInput(
                (input.RoleName ?? string.Empty).Trim(),
                StringHelper.NormalizeOptionalString(input.Description),
                input.IsActive);
        }

        public static NormalizedUserRolePermissionInput NormalizeInput(UserRolePermissionSaveInput input)
        {
            var permissionIds = (input.SelectedPermissionIds ?? new List<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            return new NormalizedUserRolePermissionInput(input.UserRoleID, permissionIds);
        }

        public static IQueryable<UserRoleEntity> ApplyFilters(
            IQueryable<UserRoleEntity> query,
            NormalizedUserRoleListInput normalized)
        {
            query = query.Where(r => r.Cancelled != true);

            if (normalized.SearchLower != null)
            {
                var s = normalized.SearchLower;
                query = query.Where(r => r.RoleName.ToLower().Contains(s));
            }

            if (normalized.FilterIds.Count > 0)
            {
                query = query.Where(r => normalized.FilterIds.Contains(r.ID_UserRole));
            }

            return query;
        }

        public static IQueryable<UserRoleEntity> ApplySorting(
            IQueryable<UserRoleEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(UserRoleSortColumn), sortColumn))
            {
                return query.OrderByDescending(r => r.ID_UserRole);
            }

            var column = (UserRoleSortColumn)sortColumn;

            switch (column)
            {
                case UserRoleSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(r => r.RoleName)
                        : query.OrderBy(r => r.RoleName);
                case UserRoleSortColumn.Description:
                    return desc
                        ? query.OrderByDescending(r => r.Description)
                        : query.OrderBy(r => r.Description);
                case UserRoleSortColumn.CreatedDate:
                    return desc
                        ? query.OrderByDescending(r => r.ID_UserRole)
                        : query.OrderBy(r => r.ID_UserRole);
                case UserRoleSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(r => r.ID_UserRole)
                        : query.OrderBy(r => r.ID_UserRole);
            }
        }

        private static readonly (string Key, string Name, string[] Modules)[] PermissionUiGroups =
        {
            ("dashboard", "Dashboard", new[] { "Dashboard" }),
            ("catalog", "Catalog", new[] { "Categories", "SubCategories", "Brands", "Products", "Variants", "VariantValues", "ProductVariants" }),
            ("purchase", "Purchase & Inventory", new[] { "Suppliers", "Purchases", "Stock" }),
            ("sales", "Sales", new[] { "Orders", "Sales", "SalesReturns", "Billing" }),
            ("customers", "Customers", new[] { "Customers", "Ratings" }),
            ("administration", "Administration", new[] { "Employees", "UserRoles" })
        };

        public static List<PermissionGroupNode> BuildPermissionGroups(IReadOnlyList<ModulePermissionNode> modules)
        {
            var remaining = modules.ToList();
            var groups = new List<PermissionGroupNode>();

            foreach (var spec in PermissionUiGroups)
            {
                var features = new List<ModulePermissionNode>();
                foreach (var moduleName in spec.Modules)
                {
                    var feature = remaining.FirstOrDefault(m =>
                        string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));
                    if (feature == null)
                    {
                        continue;
                    }

                    features.Add(feature);
                    remaining.Remove(feature);
                }

                if (features.Count == 0)
                {
                    continue;
                }

                groups.Add(new PermissionGroupNode
                {
                    GroupKey = spec.Key,
                    GroupName = spec.Name,
                    Features = features
                });
            }

            if (remaining.Count > 0)
            {
                groups.Add(new PermissionGroupNode
                {
                    GroupKey = "other",
                    GroupName = "Other",
                    Features = remaining
                });
            }

            return groups;
        }

        public static TableOutput<UserRole> EmptyTableOutput(UserRoleListInput? input)
        {
            var pageIndex = Math.Max(1, input?.PageIndex ?? 1);
            var pageSize = Math.Max(1, input?.PageSize ?? 10);

            return new TableOutput<UserRole>
            {
                TableData = new List<UserRole>(),
                TableSettings = new TableOutput_Settings
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = 0
                }
            };
        }
    }
}

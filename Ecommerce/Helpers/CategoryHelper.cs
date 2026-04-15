using Ecommerce.Helpers.Common;
using Ecommerce.Models;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Categories
{
    public sealed record NormalizedCategoryListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedCategoryWriteInput(string Name, string? Description, bool IsActive);

    /// <summary>
    /// Category list/query normalization, filtering, and sorting (no I/O).
    /// </summary>
    public static class CategoryHelper
    {
        public static NormalizedCategoryListInput NormalizeInput(CategoryListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = null;
            if (rawSearch.Length >= 1)
            {
                searchLower = rawSearch.ToLowerInvariant();
            }

            var filterIds = StringHelper.ParseFilterIds(input.FilterCategoryIDs);
            var sortColumn = input.SortColumn;
            var sortMode = input.SortMode?.Trim() ?? string.Empty;

            return new NormalizedCategoryListInput(searchLower, filterIds, pageIndex, pageSize, sortColumn, sortMode);
        }

        public static NormalizedCategoryWriteInput NormalizeInput(CategoryUpdateInput input)
        {
            var name = (input.CategoryName ?? string.Empty).Trim();
            var description = StringHelper.NormalizeOptionalString(input.Description);
            return new NormalizedCategoryWriteInput(name, description, input.IsActive);
        }

        public static IQueryable<CategoryEntity> ApplyFilters(
            IQueryable<CategoryEntity> query,
            NormalizedCategoryListInput normalized)
        {
            query = query.Where(c => c.Cancelled != true);

            if (normalized.SearchLower != null)
            {
                var s = normalized.SearchLower;
                query = query.Where(c => c.Name.ToLower().Contains(s));
            }

            if (normalized.FilterIds.Count > 0)
            {
                query = query.Where(c => normalized.FilterIds.Contains(c.ID_Category));
            }

            return query;
        }

        public static IQueryable<CategoryEntity> ApplySorting(
            IQueryable<CategoryEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(CategorySortColumn), sortColumn))
            {
                return query.OrderByDescending(c => c.ID_Category);
            }

            var column = (CategorySortColumn)sortColumn;

            switch (column)
            {
                case CategorySortColumn.Name:
                    return desc
                        ? query.OrderByDescending(c => c.Name)
                        : query.OrderBy(c => c.Name);
                case CategorySortColumn.Description:
                    return desc
                        ? query.OrderByDescending(c => c.Description)
                        : query.OrderBy(c => c.Description);
                case CategorySortColumn.CreatedDate:
                    return desc
                        ? query.OrderByDescending(c => c.ID_Category)
                        : query.OrderBy(c => c.ID_Category);
                case CategorySortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(c => c.ID_Category)
                        : query.OrderBy(c => c.ID_Category);
            }
        }

        public static TableOutput<Category> EmptyTableOutput(CategoryListInput? input)
        {
            var pageIndex = Math.Max(1, input?.PageIndex ?? 1);
            var pageSize = Math.Max(1, input?.PageSize ?? 10);

            return new TableOutput<Category>
            {
                TableData = new List<Category>(),
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

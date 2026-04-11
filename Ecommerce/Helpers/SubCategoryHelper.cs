using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.SubCategoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.SubCategories
{
    public sealed record NormalizedSubCategoryListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterCategoryIds,
        IReadOnlyList<int> FilterSubCategoryIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedSubCategoryWriteInput(string Name, int FK_Category, string? Description, bool IsActive);

    public static class SubCategoryHelper
    {
        public static NormalizedSubCategoryListInput NormalizeInput(SubCategoryListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var raw = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = raw.Length > 0 ? raw.ToLowerInvariant() : null;
            return new NormalizedSubCategoryListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterCategoryIDs),
                StringHelper.ParseFilterIds(input.FilterSubCategoryIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedSubCategoryWriteInput NormalizeInput(SubCategoryUpdateInput input)
        {
            return new NormalizedSubCategoryWriteInput(
                (input.SubCategoryName ?? string.Empty).Trim(),
                input.FK_Category,
                StringHelper.NormalizeOptionalString(input.Description),
                input.IsActive);
        }

        public static IQueryable<SubCategoryEntity> ApplyFilters(
            IQueryable<SubCategoryEntity> query,
            NormalizedSubCategoryListInput n)
        {
            query = query.Where(s => s.Cancelled != true);

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            if (n.FilterSubCategoryIds.Count > 0)
            {
                query = query.Where(x => n.FilterSubCategoryIds.Contains(x.ID_SubCategory));
            }
            else if (n.FilterCategoryIds.Count > 0)
            {
                query = query.Where(x => n.FilterCategoryIds.Contains(x.FK_Category));
            }

            return query;
        }

        public static IQueryable<SubCategoryEntity> ApplySorting(
            IQueryable<SubCategoryEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(SubCategorySortColumn), sortColumn))
            {
                return query.OrderByDescending(s => s.ID_SubCategory);
            }

            var column = (SubCategorySortColumn)sortColumn;

            switch (column)
            {
                case SubCategorySortColumn.Name:
                    return desc
                        ? query.OrderByDescending(s => s.Name)
                        : query.OrderBy(s => s.Name);
                case SubCategorySortColumn.CategoryId:
                    return desc
                        ? query.OrderByDescending(s => s.FK_Category)
                        : query.OrderBy(s => s.FK_Category);
                case SubCategorySortColumn.CreatedDate:
                    return desc
                        ? query.OrderByDescending(s => s.ID_SubCategory)
                        : query.OrderBy(s => s.ID_SubCategory);
                case SubCategorySortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(s => s.ID_SubCategory)
                        : query.OrderBy(s => s.ID_SubCategory);
            }
        }

        public static TableOutput<SubCategory> EmptyTableOutput(SubCategoryListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 10);
            return new TableOutput<SubCategory>
            {
                TableData = new List<SubCategory>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}

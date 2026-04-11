using Ecommerce.Helpers.Common;
using Ecommerce.Models;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.BrandModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Brands
{
    public sealed record NormalizedBrandListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedBrandWriteInput(string Name);

    /// <summary>
    /// Brand list/query normalization, filtering, and sorting (no I/O).
    /// </summary>
    public static class BrandHelper
    {
        public static NormalizedBrandListInput NormalizeInput(BrandListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = null;
            if (rawSearch.Length >= 2)
            {
                searchLower = rawSearch.ToLowerInvariant();
            }

            var filterIds = StringHelper.ParseFilterIds(input.FilterBrandIDs);
            var sortColumn = input.SortColumn;
            var sortMode = input.SortMode?.Trim() ?? string.Empty;

            return new NormalizedBrandListInput(searchLower, filterIds, pageIndex, pageSize, sortColumn, sortMode);
        }

        public static NormalizedBrandWriteInput NormalizeInput(BrandUpdateInput input)
        {
            var name = (input.BrandName ?? string.Empty).Trim();
            return new NormalizedBrandWriteInput(name);
        }

        public static IQueryable<BrandEntity> ApplyFilters(
            IQueryable<BrandEntity> query,
            NormalizedBrandListInput normalized)
        {
            if (normalized.SearchLower != null)
            {
                var s = normalized.SearchLower;
                query = query.Where(b => b.BrandName.ToLower().Contains(s));
            }

            if (normalized.FilterIds.Count > 0)
            {
                query = query.Where(b => normalized.FilterIds.Contains(b.BrandId));
            }

            return query;
        }

        public static IQueryable<BrandEntity> ApplySorting(
            IQueryable<BrandEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(BrandSortColumn), sortColumn))
            {
                return query.OrderByDescending(b => b.BrandId);
            }

            var column = (BrandSortColumn)sortColumn;

            switch (column)
            {
                case BrandSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(b => b.BrandName)
                        : query.OrderBy(b => b.BrandName);
                case BrandSortColumn.CancelledOn:
                    return desc
                        ? query.OrderByDescending(b => b.CancelledOn)
                        : query.OrderBy(b => b.CancelledOn);
                case BrandSortColumn.CancelledReason:
                    return desc
                        ? query.OrderByDescending(b => b.CancelledReason)
                        : query.OrderBy(b => b.CancelledReason);
                case BrandSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(b => b.BrandId)
                        : query.OrderBy(b => b.BrandId);
            }
        }

        public static TableOutput<Brand> EmptyTableOutput(BrandListInput? input)
        {
            var pageIndex = Math.Max(1, input?.PageIndex ?? 1);
            var pageSize = Math.Max(1, input?.PageSize ?? 10);

            return new TableOutput<Brand>
            {
                TableData = new List<Brand>(),
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

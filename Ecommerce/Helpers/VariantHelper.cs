using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.VariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Variants
{
    public sealed record NormalizedVariantListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedVariantWriteInput(string Name, string? Description, int DisplayOrder, bool IsActive);

    public static class VariantHelper
    {
        public static NormalizedVariantListInput NormalizeInput(VariantListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = null;
            if (rawSearch.Length >= 1)
            {
                searchLower = rawSearch.ToLowerInvariant();
            }

            var filterIds = StringHelper.ParseFilterIds(input.FilterVariantIDs);
            var sortColumn = input.SortColumn;
            var sortMode = input.SortMode?.Trim() ?? string.Empty;

            return new NormalizedVariantListInput(searchLower, filterIds, pageIndex, pageSize, sortColumn, sortMode);
        }

        public static NormalizedVariantWriteInput NormalizeInput(VariantUpdateInput input)
        {
            var name = (input.Name ?? string.Empty).Trim();
            var description = StringHelper.NormalizeOptionalString(input.Description);
            var displayOrder = Math.Max(0, input.DisplayOrder);
            return new NormalizedVariantWriteInput(name, description, displayOrder, input.IsActive);
        }

        public static IQueryable<VariantEntity> ApplyFilters(
            IQueryable<VariantEntity> query,
            NormalizedVariantListInput normalized)
        {
            query = query.Where(v => v.Cancelled != true);

            if (normalized.SearchLower != null)
            {
                var s = normalized.SearchLower;
                query = query.Where(v => v.Name.ToLower().Contains(s));
            }

            if (normalized.FilterIds.Count > 0)
            {
                query = query.Where(v => normalized.FilterIds.Contains(v.ID_Variant));
            }

            return query;
        }

        public static IQueryable<VariantEntity> ApplySorting(
            IQueryable<VariantEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(VariantSortColumn), sortColumn))
            {
                return query.OrderByDescending(v => v.ID_Variant);
            }

            var column = (VariantSortColumn)sortColumn;

            switch (column)
            {
                case VariantSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(v => v.Name)
                        : query.OrderBy(v => v.Name);
                case VariantSortColumn.DisplayOrder:
                    return desc
                        ? query.OrderByDescending(v => v.DisplayOrder)
                        : query.OrderBy(v => v.DisplayOrder);
                case VariantSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(v => v.ID_Variant)
                        : query.OrderBy(v => v.ID_Variant);
            }
        }

        public static TableOutput<Variant> EmptyTableOutput(VariantListInput? input)
        {
            var pageIndex = Math.Max(1, input?.PageIndex ?? 1);
            var pageSize = Math.Max(1, input?.PageSize ?? 10);

            return new TableOutput<Variant>
            {
                TableData = new List<Variant>(),
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

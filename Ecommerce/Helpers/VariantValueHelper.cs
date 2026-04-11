using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.VariantValueModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.VariantValues
{
    public sealed record NormalizedVariantValueListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterVariantIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedVariantValueWriteInput(
        int FkVariant,
        string Name,
        string? Description,
        string? ValueIcon,
        int DisplayOrder);

    public static class VariantValueHelper
    {
        public static NormalizedVariantValueListInput NormalizeInput(VariantValueListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var raw = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = raw.Length >= 2 ? raw.ToLowerInvariant() : null;
            return new NormalizedVariantValueListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterVariantIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedVariantValueWriteInput NormalizeInput(VariantValueUpdateInput input)
        {
            return new NormalizedVariantValueWriteInput(
                input.FK_Variant,
                (input.ValueName ?? string.Empty).Trim(),
                StringHelper.NormalizeOptionalString(input.Description),
                StringHelper.NormalizeOptionalString(input.ValueIcon),
                input.DisplayOrder <= 0 ? 1 : input.DisplayOrder);
        }

        public static IQueryable<VariantValueEntity> ApplyFilters(
            IQueryable<VariantValueEntity> query,
            NormalizedVariantValueListInput n)
        {
            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(vv => vv.ValueName.ToLower().Contains(s));
            }

            if (n.FilterVariantIds.Count > 0)
            {
                query = query.Where(vv => n.FilterVariantIds.Contains(vv.FkVariant));
            }

            return query;
        }

        public static IQueryable<VariantValueEntity> ApplySorting(
            IQueryable<VariantValueEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(VariantValueSortColumn), sortColumn))
            {
                return desc
                    ? query.OrderByDescending(vv => vv.IdVariantValue)
                    : query.OrderBy(vv => vv.IdVariantValue);
            }

            var column = (VariantValueSortColumn)sortColumn;

            switch (column)
            {
                case VariantValueSortColumn.ValueName:
                    return desc
                        ? query.OrderByDescending(vv => vv.ValueName)
                        : query.OrderBy(vv => vv.ValueName);
                case VariantValueSortColumn.DisplayOrder:
                    return desc
                        ? query.OrderByDescending(vv => vv.DisplayOrder)
                        : query.OrderBy(vv => vv.DisplayOrder);
                case VariantValueSortColumn.CreatedOn:
                    return desc
                        ? query.OrderByDescending(vv => vv.CreatedOn)
                        : query.OrderBy(vv => vv.CreatedOn);
                case VariantValueSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(vv => vv.IdVariantValue)
                        : query.OrderBy(vv => vv.IdVariantValue);
            }
        }

        public static TableOutput<VariantValueListOutput> EmptyTableOutput(VariantValueListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 10);
            return new TableOutput<VariantValueListOutput>
            {
                TableData = new List<VariantValueListOutput>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}

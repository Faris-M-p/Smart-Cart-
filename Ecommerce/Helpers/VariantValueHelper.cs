using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.VariantValueModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.VariantValues
{
    public sealed record NormalizedVariantValueListInput(
        string? SearchLower,
        int FkVariantFilter,
        IReadOnlyList<int> FilterVariantIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedVariantValueCreate(int FkVariant, string Name, string? Description, int DisplayOrder);

    public sealed record NormalizedVariantValueUpdate(int Id, string Name, string? Description, int DisplayOrder);

    public static class VariantValueHelper
    {
        public static NormalizedVariantValueListInput NormalizeInput(VariantValueListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var raw = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = raw.Length >= 1 ? raw.ToLowerInvariant() : null;
            var fkVariantFilter = Math.Max(0, input.FK_Variant);
            return new NormalizedVariantValueListInput(
                searchLower,
                fkVariantFilter,
                StringHelper.ParseFilterIds(input.FilterVariantIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedVariantValueCreate NormalizeCreateInput(VariantValueCreateInput input)
        {
            var name = (input.Name ?? string.Empty).Trim();
            var description = StringHelper.NormalizeOptionalString(input.Description);
            var order = Math.Max(0, input.DisplayOrder);
            return new NormalizedVariantValueCreate(input.FK_Variant, name, description, order);
        }

        public static NormalizedVariantValueUpdate NormalizeUpdateInput(VariantValueUpdateInput input)
        {
            var name = (input.Name ?? string.Empty).Trim();
            var description = StringHelper.NormalizeOptionalString(input.Description);
            var order = Math.Max(0, input.DisplayOrder);
            return new NormalizedVariantValueUpdate(input.VariantValueID, name, description, order);
        }

        public static IQueryable<VariantValueEntity> ApplyFilters(
            IQueryable<VariantValueEntity> query,
            NormalizedVariantValueListInput n)
        {
            query = query.Where(vv => vv.Cancelled != true);

            if (n.FkVariantFilter > 0)
            {
                query = query.Where(vv => vv.FkVariant == n.FkVariantFilter);
            }
            else if (n.FilterVariantIds.Count > 0)
            {
                query = query.Where(vv => n.FilterVariantIds.Contains(vv.FkVariant));
            }

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(vv => vv.Name.ToLower().Contains(s));
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
                case VariantValueSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(vv => vv.Name)
                        : query.OrderBy(vv => vv.Name);
                case VariantValueSortColumn.DisplayOrder:
                    return desc
                        ? query.OrderByDescending(vv => vv.DisplayOrder)
                        : query.OrderBy(vv => vv.DisplayOrder);
                case VariantValueSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(vv => vv.IdVariantValue)
                        : query.OrderBy(vv => vv.IdVariantValue);
            }
        }

        public static TableOutput<VariantValue> EmptyTableOutput(VariantValueListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 10);
            return new TableOutput<VariantValue>
            {
                TableData = new List<VariantValue>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}

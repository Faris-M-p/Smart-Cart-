using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.SupplierModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Suppliers
{
    public sealed record NormalizedSupplierListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterSupplierIds,
        bool ShowCancelled,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedSupplierWriteInput(
        string Name,
        string? Email,
        string? Phone,
        string? Address);

    public static class SupplierHelper
    {
        public static NormalizedSupplierListInput NormalizeInput(SupplierListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var raw = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = raw.Length >= 1 ? raw.ToLowerInvariant() : null;
            return new NormalizedSupplierListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterSupplierIDs),
                input.ShowCancelled,
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedSupplierWriteInput NormalizeInput(SupplierUpdateInput input)
        {
            return new NormalizedSupplierWriteInput(
                (input.SupplierName ?? string.Empty).Trim(),
                StringHelper.NormalizeOptionalString(input.Email),
                StringHelper.TruncateOptional(input.Phone, 15),
                StringHelper.NormalizeOptionalString(input.Address));
        }

        public static IQueryable<SupplierEntity> ApplyFilters(
            IQueryable<SupplierEntity> query,
            NormalizedSupplierListInput n)
        {
            query = query.Where(s => s.Cancelled != true);

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(x =>
                    x.SupplierName.ToLower().Contains(s) ||
                    (x.ContactEmail != null && x.ContactEmail.ToLower().Contains(s)) ||
                    (x.ContactPhone != null && x.ContactPhone.ToLower().Contains(s)) ||
                    (x.Address != null && x.Address.ToLower().Contains(s)));
            }

            if (n.FilterSupplierIds.Count > 0)
            {
                query = query.Where(x => n.FilterSupplierIds.Contains(x.SupplierId));
            }

            return query;
        }

        public static IQueryable<SupplierEntity> ApplySorting(
            IQueryable<SupplierEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(SupplierSortColumn), sortColumn))
            {
                return desc
                    ? query.OrderByDescending(s => s.SupplierId)
                    : query.OrderBy(s => s.SupplierId);
            }

            var column = (SupplierSortColumn)sortColumn;

            switch (column)
            {
                case SupplierSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(s => s.SupplierName)
                        : query.OrderBy(s => s.SupplierName);
                case SupplierSortColumn.CreatedAt:
                    return desc
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt);
                case SupplierSortColumn.Email:
                    return desc
                        ? query.OrderByDescending(s => s.ContactEmail)
                        : query.OrderBy(s => s.ContactEmail);
                case SupplierSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(s => s.SupplierId)
                        : query.OrderBy(s => s.SupplierId);
            }
        }

        public static TableOutput<Supplier> EmptyTableOutput(SupplierListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 20);
            return new TableOutput<Supplier>
            {
                TableData = new List<Supplier>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}

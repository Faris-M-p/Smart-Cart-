using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.PurchaseModel;

namespace Ecommerce.Helpers.Purchases
{
    public sealed record NormalizedPurchaseListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterSupplierIds,
        DateTime? FromDate,
        DateTime? ToDate,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public static class PurchaseHelper
    {
        public static NormalizedPurchaseListInput NormalizeInput(PurchaseListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var sortMode = string.IsNullOrWhiteSpace(input.SortMode) ? "DESC" : input.SortMode.Trim();

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = rawSearch.Length >= 1 ? rawSearch.ToLowerInvariant() : null;

            return new NormalizedPurchaseListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterSupplierIDs),
                input.FromDate,
                input.ToDate,
                pageIndex,
                pageSize,
                input.SortColumn,
                sortMode);
        }

        public static IQueryable<PurchaseEntity> ApplyFilters(
            IQueryable<PurchaseEntity> query,
            NormalizedPurchaseListInput normalized)
        {
            query = query.Where(p => p.Cancelled != true);

            if (normalized.FromDate.HasValue)
            {
                var from = normalized.FromDate.Value.Date;
                query = query.Where(p => p.PurchaseDate.Date >= from);
            }

            if (normalized.ToDate.HasValue)
            {
                var to = normalized.ToDate.Value.Date;
                query = query.Where(p => p.PurchaseDate.Date <= to);
            }

            if (normalized.FilterSupplierIds.Count > 0)
            {
                query = query.Where(p => normalized.FilterSupplierIds.Contains(p.FK_Supplier));
            }

            if (normalized.SearchLower != null)
            {
                var s = normalized.SearchLower;
                query = query.Where(p =>
                    (p.InvoiceNumber ?? string.Empty).ToLower().Contains(s));
            }

            return query;
        }

        public static IQueryable<PurchaseEntity> ApplySorting(
            IQueryable<PurchaseEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(PurchaseSortColumn), sortColumn))
            {
                return desc ? query.OrderByDescending(p => p.ID_Purchase) : query.OrderBy(p => p.ID_Purchase);
            }

            var col = (PurchaseSortColumn)sortColumn;
            switch (col)
            {
                case PurchaseSortColumn.PurchaseDate:
                    return desc ? query.OrderByDescending(p => p.PurchaseDate) : query.OrderBy(p => p.PurchaseDate);
                case PurchaseSortColumn.TotalAmount:
                    return desc ? query.OrderByDescending(p => p.TotalAmount) : query.OrderBy(p => p.TotalAmount);
                case PurchaseSortColumn.InvoiceNumber:
                    return desc ? query.OrderByDescending(p => p.InvoiceNumber) : query.OrderBy(p => p.InvoiceNumber);
                case PurchaseSortColumn.Id:
                default:
                    return desc ? query.OrderByDescending(p => p.ID_Purchase) : query.OrderBy(p => p.ID_Purchase);
            }
        }

        public static TableOutput<Purchase> EmptyTableOutput(PurchaseListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 20);
            return new TableOutput<Purchase>
            {
                TableData = new List<Purchase>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}

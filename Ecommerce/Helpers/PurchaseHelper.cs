using Ecommerce.Models.Enums;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.PurchaseModel;

namespace Ecommerce.Helpers.Purchases
{
    public static class PurchaseHelper
    {
        public static object BuildPurchaseListSpParameters(PurchaseListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var sortMode = string.IsNullOrWhiteSpace(input.SortMode) ? "DESC" : input.SortMode.Trim();
            var sortColumn = MapPurchaseSortColumnForSp(input.SortColumn);

            return new
            {
                SearchText = input.SearchText ?? string.Empty,
                FilterSupplierIDs = input.FilterSupplierIDs ?? string.Empty,
                input.FromDate,
                input.ToDate,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortMode = sortMode
            };
        }

        public static string MapPurchaseSortColumnForSp(int sortColumn)
        {
            if (!Enum.IsDefined(typeof(PurchaseSortColumn), sortColumn))
            {
                return "ID_Purchase";
            }

            return (PurchaseSortColumn)sortColumn switch
            {
                PurchaseSortColumn.PurchaseDate => "PurchaseDate",
                PurchaseSortColumn.TotalAmount => "TotalAmount",
                PurchaseSortColumn.InvoiceNumber => "InvoiceNumber",
                PurchaseSortColumn.Id => "ID_Purchase",
                _ => "ID_Purchase"
            };
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

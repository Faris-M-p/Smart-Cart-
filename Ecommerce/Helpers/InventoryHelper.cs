using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.InventoryModel;

namespace Ecommerce.Helpers.Inventory
{
    public sealed record NormalizedStockListInput(
        string? SearchLower,
        bool LowStockOnly,
        int PageIndex,
        int PageSize);

    public static class InventoryHelper
    {
        public static NormalizedStockListInput NormalizeInput(StockListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Clamp(input.PageSize, 1, 100);

            var raw = (input.SearchText ?? string.Empty).Trim();
            var searchLower = raw.Length >= 1 ? raw.ToLowerInvariant() : null;

            return new NormalizedStockListInput(searchLower, input.LowStockOnly, pageIndex, pageSize);
        }

        public static TableOutput<StockRow> EmptyTableOutput(StockListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 20);
            return new TableOutput<StockRow>
            {
                TableData = new List<StockRow>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}


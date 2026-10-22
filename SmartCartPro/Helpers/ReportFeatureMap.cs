namespace SmartCartPro.Helpers;

/// <summary>Maps a report action name to the leaf feature that gates it.</summary>
public static class ReportFeatureMap
{
    private static readonly Dictionary<string, string> Actions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Sales"] = "REPORTS.SALES.BILL_WISE",
        ["SalesCsv"] = "REPORTS.SALES.BILL_WISE",
        ["SalesBillWise"] = "REPORTS.SALES.BILL_WISE",
        ["Products"] = "REPORTS.SALES.ITEM",
        ["ProductsCsv"] = "REPORTS.SALES.ITEM",
        ["ItemSales"] = "REPORTS.SALES.ITEM",
        ["CategorySales"] = "REPORTS.SALES.CATEGORY",
        ["SalesReturns"] = "REPORTS.SALES.RETURNS",
        ["DayClose"] = "REPORTS.SALES.DAY_CLOSE",
        ["DiscountReport"] = "REPORTS.SALES.DISCOUNT",
        ["CancelledBills"] = "REPORTS.SALES.CANCELLED",
        ["HourlySales"] = "REPORTS.SALES.HOURLY",
        ["StockValuation"] = "REPORTS.STOCK.VALUATION",
        ["StockValuationCsv"] = "REPORTS.STOCK.VALUATION",
        ["CurrentStock"] = "REPORTS.STOCK.VALUATION",
        ["ItemLedger"] = "REPORTS.STOCK.ITEM_LEDGER",
        ["StockLedger"] = "REPORTS.STOCK.STOCK_LEDGER",
        ["StockMovement"] = "REPORTS.STOCK.MOVEMENT",
        ["NonMoving"] = "REPORTS.STOCK.NON_MOVING",
        ["LowStockReport"] = "REPORTS.STOCK.LOW_STOCK",
        ["StockAdjustments"] = "REPORTS.STOCK.ADJUSTMENTS",
        ["Purchases"] = "REPORTS.PURCHASE.BILLS",
        ["PurchasesCsv"] = "REPORTS.PURCHASE.BILLS",
        ["PurchaseBills"] = "REPORTS.PURCHASE.BILLS",
        ["PurchaseAnalysis"] = "REPORTS.PURCHASE.ANALYSIS",
        ["Tax"] = "REPORTS.TAX.GST",
        ["TaxCsv"] = "REPORTS.TAX.GST",
        ["GstSummary"] = "REPORTS.TAX.GST",
        ["ProfitMargin"] = "REPORTS.PROFIT.MARGIN",
        ["Customers"] = "REPORTS.PARTIES.CUSTOMERS",
        ["CustomersCsv"] = "REPORTS.PARTIES.CUSTOMERS",
        ["MasterLists"] = "REPORTS.PARTIES.MASTERS"
    };

    public static string? ForAction(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
            return null;
        var name = action;
        foreach (var suffix in new[] { "Data", "Export", "Print", "Csv" })
        {
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) && name.Length > suffix.Length)
            {
                var stem = name[..^suffix.Length];
                if (Actions.ContainsKey(stem))
                    name = stem;
                break;
            }
        }

        return Actions.TryGetValue(name, out var code) ? code : null;
    }

    public static string? ActionFromHref(string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return null;
        const string marker = "/Reports/";
        var index = href.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return null;
        var rest = href[(index + marker.Length)..];
        var end = rest.IndexOfAny(new[] { '?', '#', '/' });
        var action = end < 0 ? rest : rest[..end];
        return string.IsNullOrWhiteSpace(action) ? null : action;
    }
}

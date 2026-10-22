using SmartCartPro.Models;

namespace SmartCartPro.Services;

internal sealed class ReportCatalog
{
    internal sealed record Choice(string Value, string Label);

    internal sealed class Def
    {
        public string Key { get; init; } = "";
        public string Action { get; init; } = "";
        public string Section { get; init; } = "";
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public string Icon { get; init; } = "bi-file-earmark-bar-graph";
        public string Range { get; init; } = "today";
        public bool ProfitOnly { get; init; }
        public string SearchLabel { get; init; } = "Item";
        public HashSet<string> Filters { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public List<Choice> Status { get; set; } = new();
        public List<Choice> Level { get; set; } = new();
        public List<Choice> Group { get; set; } = new();
        public List<Choice> Movement { get; set; } = new();
        public List<Choice> Days { get; set; } = new();
    }

    private static Def D(
        string key, string section, string title, string description, string icon, string range,
        string filters, bool profitOnly = false, string searchLabel = "Item") => new()
    {
        Key = key,
        Action = key,
        Section = section,
        Title = title,
        Description = description,
        Icon = icon,
        Range = range,
        ProfitOnly = profitOnly,
        SearchLabel = searchLabel,
        Filters = filters.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase)
    };

    internal static IReadOnlyList<Def> All { get; } = new List<Def>
    {
        With(D("SalesBillWise", "Sales", "Sales report", "Bills, tax, discount and payments", "bi-receipt", "today",
            "dates store cashier customer payment status"),
            status: new[] { C("completed", "Completed"), C("cancelled", "Cancelled"), C("all", "All") }),
        D("ItemSales", "Sales", "Item-wise sales", "Quantity and value by item", "bi-box", "today",
            "dates store category subcategory brand search"),
        With(D("CategorySales", "Sales", "Category-wise sales", "Bills, quantity and share of sales", "bi-diagram-3", "today",
            "dates store level"),
            level: new[] { C("category", "Category"), C("subcategory", "Sub-category") }),
        D("SalesReturns", "Sales", "Sales returns", "Return notes, quantity and refund amount", "bi-arrow-counterclockwise", "today",
            "dates store cashier customer"),
        D("DayClose", "Sales", "Day close", "Cashier sessions and tender totals", "bi-cash-stack", "today",
            "dates store cashier"),
        With(D("DiscountReport", "Sales", "Discount report", "Bills and items with a discount", "bi-percent", "today",
            "dates store group"),
            group: new[] { C("bill", "By bill"), C("cashier", "By cashier"), C("item", "By item") }),
        D("CancelledBills", "Sales", "Cancelled bills", "Bills cancelled in the selected dates", "bi-x-circle", "today",
            "dates store cashier"),
        With(D("HourlySales", "Sales", "Hourly sales", "Bills and net value by hour of day", "bi-clock", "today",
            "dates store group"),
            group: new[] { C("hour", "Hour of day"), C("dow", "Day of week") }),

        D("CurrentStock", "Stock", "Current stock", "On-hand quantity and selling price", "bi-boxes", "none",
            "store category subcategory brand search zero negative"),
        D("ItemLedger", "Stock", "Item ledger", "Opening, movements and closing for one item", "bi-journal-text", "month",
            "dates store search"),
        D("StockLedger", "Stock", "Stock ledger", "Opening, in, out and closing by item", "bi-table", "month",
            "dates store category search"),
        With(D("StockMovement", "Stock", "Stock inward / outward", "Movement totals or the document list", "bi-arrow-left-right", "month",
            "dates store movement group"),
            movement: MoveTypes(),
            group: new[] { C("type", "By type"), C("document", "By document") }),
        D("LowStockReport", "Stock", "Low stock", "Items at or below reorder level", "bi-exclamation-triangle", "none",
            "store category supplier"),
        With(D("NonMoving", "Stock", "Non-moving items", "Items not sold in the selected days", "bi-hourglass", "none",
            "store category days instock"),
            days: new[] { C("15", "15 days"), C("30", "30 days"), C("60", "60 days"), C("90", "90 days") }),
        D("StockAdjustments", "Stock", "Adjustments and transfers", "Adjustment and transfer movements", "bi-shuffle", "month",
            "dates store search"),

        With(D("PurchaseBills", "Purchase", "Purchase report", "Purchase bills, tax and status", "bi-truck", "month",
            "dates store supplier status"),
            status: new[] { C("all", "All"), C("posted", "Posted"), C("partially_received", "Partially received"), C("received", "Received"), C("draft", "Draft"), C("cancelled", "Cancelled") }),
        With(D("PurchaseAnalysis", "Purchase", "Supplier and item purchases", "Purchase totals by supplier or item", "bi-bar-chart", "month",
            "dates store supplier category group"),
            group: new[] { C("supplier", "Supplier-wise"), C("item", "Item-wise") }),

        D("GstSummary", "Tax", "GST sales summary", "Taxable value and tax by rate, net of return value", "bi-percent", "month",
            "dates store"),
        With(D("ProfitMargin", "Profit", "Profit and margin", "Sales, current cost and gross profit", "bi-graph-up", "today",
            "dates store group", profitOnly: true),
            group: new[] { C("item", "Item-wise"), C("category", "Category-wise"), C("bill", "Bill-wise") }),

        With(D("MasterLists", "Parties & Masters", "Master lists", "Customers, suppliers, employees and categories", "bi-people", "none",
            "group search", searchLabel: "Search"),
            group: new[] { C("customers", "Customers"), C("suppliers", "Suppliers"), C("employees", "Employees"), C("categories", "Categories") })
    };

    internal static Def? Find(string key) =>
        All.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));

    private static Choice C(string value, string label) => new(value, label);

    private static Def With(Def def, Choice[]? status = null, Choice[]? level = null, Choice[]? group = null, Choice[]? movement = null, Choice[]? days = null)
    {
        if (status is not null) def.Status = status.ToList();
        if (level is not null) def.Level = level.ToList();
        if (group is not null) def.Group = group.ToList();
        if (movement is not null) def.Movement = movement.ToList();
        if (days is not null) def.Days = days.ToList();
        return def;
    }

    private static Choice[] MoveTypes() => new[]
    {
        C("", "All types"),
        C("purchase", "Purchase"),
        C("sale", "Sale"),
        C("return_in", "Sales return in"),
        C("return_out", "Purchase return out"),
        C("adjustment", "Adjustment"),
        C("transfer_in", "Transfer in"),
        C("transfer_out", "Transfer out"),
        C("opening", "Opening")
    };
}

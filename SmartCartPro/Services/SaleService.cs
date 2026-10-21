using System.Globalization;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class SaleService : ISaleService
{
    private const int MaxLines = 200;
    private const decimal MaxQuantity = 9_999_999.999m;
    private const decimal MaxAmount = 9_999_999_999.99m;

    private static readonly int[] PageSizes = { 10, 25, 50, 100 };
    private static readonly string[] SaleStatuses = { "completed", "cancelled", "held" };
    private static readonly string[] SaleSortFields = { "id", "invoicenumber", "saledate", "customer", "totalamount", "paidamount", "status" };
    private static readonly string[] ReturnStatuses = { "completed", "cancelled" };
    private static readonly string[] ReturnSortFields = { "id", "returnnumber", "invoicenumber", "returndate", "totalamount", "status", "cashier" };
    private static readonly string[] PaymentMethods = { "cash", "card", "upi", "wallet", "bank", "other" };
    private static readonly string[] SessionStatuses = { "open", "closed" };

    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private readonly ISaleRepository _repo;
    private readonly ISettingService _settings;

    public SaleService(ISaleRepository repo, ISettingService settings)
    {
        _repo = repo;
        _settings = settings;
    }

    public async Task<OutputPosSelect> GetPosAsync(InputPosSelect input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to use the POS.");

        var page = await _repo.GetPosContextAsync(input, ct)
            ?? throw new ResourceNotFoundException(
                "The store was not found.",
                $"Store {input.FkStore} is missing or cancelled.");

        page.DefaultTaxRate = await ResolveDefaultTaxRateAsync(input.FkStore, ct);
        var currency = await _settings.GetEffectiveValueAsync("locale.currency", input.FkStore, ct);
        if (!string.IsNullOrWhiteSpace(currency))
            page.CurrencyCode = currency.Trim().ToUpperInvariant();

        return page;
    }

    public async Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default)
    {
        var sale = await _repo.GetSaleAsync(input, ct);
        if (sale is null) return null;

        sale.Notes = StripClientRef(sale.Notes);
        ApplyInvoiceRules(sale);
        if (sale.CanReturn)
        {
            var returnable = await _repo.GetReturnableSaleAsync(
                new InputSaleReturnableSelect { IdSale = sale.IdSale, FkStore = sale.FkStore }, ct);
            sale.CanReturn = returnable?.Lines.Any(l => l.RemainingQty > 0) == true;
        }
        return sale;
    }

    public async Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to view invoices.");

        input.Search = Clean(input.Search);
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];

        input.CustomerId = input.CustomerId is > 0 ? input.CustomerId : input.FkCustomer is > 0 ? input.FkCustomer : null;
        input.FkCustomer = input.CustomerId;

        var status = (input.Status ?? string.Empty).Trim().ToLowerInvariant();
        input.Status = Array.IndexOf(SaleStatuses, status) >= 0 ? status : null;

        var method = (input.PaymentMethod ?? string.Empty).Trim().ToLowerInvariant();
        input.PaymentMethod = Array.IndexOf(PaymentMethods, method) >= 0 ? method : null;

        await ApplyDateBoundsAsync(input.FkStore, input.From, input.To,
            (fromUtc, toEx) => { input.FromUtc = fromUtc; input.ToExclusiveUtc = toEx; }, ct);

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(SaleSortFields, sort) >= 0 ? sort : "saledate";

        var result = await _repo.ListSalesAsync(input, ct);
        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListSalesAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to complete a sale.");

        input.ClientRef = Clean(input.ClientRef);
        input.Notes = Clean(input.Notes);
        if (input.FkCustomer is <= 0) input.FkCustomer = null;
        foreach (var line in input.Lines)
            line.FkProductVariant = line.FkProductVariant is > 0 ? line.FkProductVariant : null;

        var session = await _repo.GetOpenSessionAsync(new InputCashSessionSelect { FkStore = input.FkStore }, ct);
        if (session is null)
            throw new ConflictException(
                "Open a cash session before completing a sale.",
                $"Store {input.FkStore} has no open cash session.");

        input.FkCashSession = session.IdCashSession;

        var errors = new Dictionary<string, List<string>>();
        await PrepareLinesAsync(input, errors, ct);
        PreparePayments(input, errors);
        ThrowIfInvalid(errors);

        var gate = GateFor(HashCode.Combine(input.FkStore, input.ClientRef ?? string.Empty));
        await gate.WaitAsync(ct);
        try
        {
            var result = await _repo.CreateSaleAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to complete the sale.");

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to cancel a sale.");

        input.Reason = (input.Reason ?? string.Empty).Trim();
        if (input.Reason.Length < 3)
        {
            const string message = "Please enter a reason of at least 3 characters.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["Reason"] = new[] { message } });
        }

        var gate = GateFor(input.IdSale);
        await gate.WaitAsync(ct);
        try
        {
            var result = await _repo.CancelSaleAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to cancel the sale.");

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to process a return.");

        input.Reason = (input.Reason ?? string.Empty).Trim();
        if (input.Reason.Length < 3)
        {
            const string message = "Please enter a reason of at least 3 characters.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["Reason"] = new[] { message } });
        }

        var errors = new Dictionary<string, List<string>>();
        var positive = 0;
        for (var i = 0; i < input.Lines.Count; i++)
        {
            var line = input.Lines[i];
            line.FkProductVariant = line.FkProductVariant is > 0 ? line.FkProductVariant : null;
            line.ReturnQty = Math.Round(line.ReturnQty, 3, MidpointRounding.AwayFromZero);
            line.UnitPrice = 0;

            if (line.ReturnQty < 0)
                AddError(errors, $"Lines[{i}].ReturnQty", $"Line {i + 1}: the return quantity cannot be negative.");
            else if (line.ReturnQty > MaxQuantity)
                AddError(errors, $"Lines[{i}].ReturnQty", $"Line {i + 1}: the return quantity is too large.");
            else if (line.ReturnQty > 0)
                positive++;
        }

        if (positive == 0)
            AddError(errors, "Lines", "Enter a return quantity for at least one line.");

        ThrowIfInvalid(errors);

        var gate = GateFor(input.IdSale);
        await gate.WaitAsync(ct);
        try
        {
            return await _repo.ReturnSaleAsync(input, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<OutputSaleReturnableSelect> GetReturnableSaleAsync(
        InputSaleReturnableSelect input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to process a return.");

        var sale = await _repo.GetReturnableSaleAsync(input, ct);
        if (sale is null)
            throw new ResourceNotFoundException(
                "Invoice not found.",
                $"Sale {input.IdSale} is missing, cancelled, or not a completed sale in store {input.FkStore}.");

        return sale;
    }

    public async Task<OutputSalesReturnList> ListSalesReturnsAsync(
        InputSalesReturnList input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to view returns.");

        input.Search = Clean(input.Search);
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];

        var status = (input.Status ?? string.Empty).Trim().ToLowerInvariant();
        input.Status = Array.IndexOf(ReturnStatuses, status) >= 0 ? status : null;

        await ApplyDateBoundsAsync(input.FkStore, input.From, input.To,
            (fromUtc, toEx) => { input.FromUtc = fromUtc; input.ToExclusiveUtc = toEx; }, ct);

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(ReturnSortFields, sort) >= 0 ? sort : "returndate";

        var result = await _repo.ListSalesReturnsAsync(input, ct);
        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListSalesReturnsAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputSalesReturnSelect?> GetSalesReturnAsync(
        InputSalesReturnSelect input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to view a return.");
        return await _repo.GetSalesReturnAsync(input, ct);
    }

    public async Task<IReadOnlyList<OutputSaleProductSelect>> SearchProductsAsync(
        InputSaleProductSearch input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to search products.");

        var term = Clean(input.Term);
        if (term is null)
            return Array.Empty<OutputSaleProductSelect>();

        input.Term = term.Length > 80 ? term[..80] : term;
        input.Take = input.Take is > 0 and <= 20 ? input.Take : 20;

        var items = await _repo.SearchProductsAsync(input, ct);
        await ApplyDefaultTaxAsync(input.FkStore, items, ct);
        return items;
    }

    public async Task<OutputSaleProductSelect> GetProductByBarcodeAsync(
        InputSaleProductBarcode input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to scan products.");

        input.Barcode = (input.Barcode ?? string.Empty).Trim();
        if (input.Barcode.Length == 0)
        {
            const string message = "Enter a barcode.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["barcode"] = new[] { message } });
        }

        if (input.Barcode.Length > 80)
            input.Barcode = input.Barcode[..80];

        var item = await _repo.GetProductByBarcodeAsync(input, ct);
        if (item is null)
            throw new ResourceNotFoundException(
                "No product matches that barcode.",
                $"Barcode '{input.Barcode}' was not found for store {input.FkStore}.");

        await ApplyDefaultTaxAsync(input.FkStore, new[] { item }, ct);
        return item;
    }

    public async Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to open a cash session.");

        input.OpeningFloat = Money(input.OpeningFloat);
        if (input.OpeningFloat < 0)
        {
            const string message = "The opening float cannot be negative.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["OpeningFloat"] = new[] { message } });
        }

        input.Notes = Clean(input.Notes);

        var gate = GateFor(input.FkStore);
        await gate.WaitAsync(ct);
        try
        {
            var result = await _repo.OpenSessionAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to open the cash session.");

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to close a cash session.");

        input.ClosingCash = Money(input.ClosingCash);
        if (input.ClosingCash < 0)
        {
            const string message = "The counted cash cannot be negative.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["ClosingCash"] = new[] { message } });
        }

        input.Notes = Clean(input.Notes);

        var gate = GateFor(input.IdCashSession);
        await gate.WaitAsync(ct);
        try
        {
            var result = await _repo.CloseSessionAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to close the cash session.");

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default)
        => _repo.GetOpenSessionAsync(input, ct);

    public async Task<OutputCashSessionList> ListSessionsAsync(InputCashSessionList input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to view cash sessions.");

        var status = (input.Status ?? string.Empty).Trim().ToLowerInvariant();
        input.Status = Array.IndexOf(SessionStatuses, status) >= 0 ? status : null;

        await ApplyDateBoundsAsync(input.FkStore, input.From, input.To,
            (fromUtc, toEx) => { input.FromUtc = fromUtc; input.ToExclusiveUtc = toEx; }, ct);

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var result = await _repo.ListSessionsAsync(input, ct);
        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListSessionsAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputCashSummary> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore, "Sign in as a store user to view the day close.");

        var summary = await _repo.GetCashSummaryAsync(input, ct);
        if (summary is null)
            throw new ResourceNotFoundException(
                "Cash session not found.",
                $"Cash session {input.IdCashSession} was not found in store {input.FkStore}.");

        return summary;
    }

    // ───────────────────────── create helpers ─────────────────────────

    private async Task PrepareLinesAsync(
        InputSaleCreate input,
        Dictionary<string, List<string>> errors,
        CancellationToken ct)
    {
        if (input.Lines.Count == 0)
        {
            AddError(errors, "Lines", "Add at least one item to the bill.");
            return;
        }

        if (input.Lines.Count > MaxLines)
        {
            AddError(errors, "Lines", $"A sale can have at most {MaxLines} lines.");
            return;
        }

        var keys = new InputSaleProductKeys
        {
            FkStore = input.FkStore,
            Keys = input.Lines.Select(l => new InputSaleProductKey
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant
            }).ToList()
        };

        var picks = await _repo.GetProductsByKeysAsync(keys, ct);
        await ApplyDefaultTaxAsync(input.FkStore, picks, ct);
        var byKey = picks.ToDictionary(p => (p.IdProduct, p.IdProductVariant ?? 0));

        decimal subTotal = 0, discountTotal = 0, taxTotal = 0;

        for (var i = 0; i < input.Lines.Count; i++)
        {
            var line = input.Lines[i];
            var prefix = $"Lines[{i}]";
            var label = $"Line {i + 1}";

            line.Quantity = Math.Round(line.Quantity, 3, MidpointRounding.AwayFromZero);
            line.DiscountAmount = Money(line.DiscountAmount);

            if (!byKey.TryGetValue((line.FkProduct, line.FkProductVariant ?? 0), out var pick))
            {
                AddError(errors, $"{prefix}.FkProduct", $"{label}: the product is not available.");
                continue;
            }

            var valid = true;
            if (line.Quantity <= 0)
            {
                AddError(errors, $"{prefix}.Quantity", $"{label}: the quantity must be greater than zero.");
                valid = false;
            }
            else if (line.Quantity > MaxQuantity)
            {
                AddError(errors, $"{prefix}.Quantity", $"{label}: the quantity is too large.");
                valid = false;
            }
            else if (!pick.AllowsFraction && line.Quantity != Math.Truncate(line.Quantity))
            {
                AddError(errors, $"{prefix}.Quantity", $"{label}: this item is sold in whole units only.");
                valid = false;
            }

            if (line.DiscountAmount < 0)
            {
                AddError(errors, $"{prefix}.DiscountAmount", $"{label}: the discount cannot be negative.");
                valid = false;
            }

            if (!valid) continue;

            var unitPrice = Money(pick.SellingPrice);
            var gross = Money(line.Quantity * unitPrice);
            if (gross > MaxAmount)
            {
                AddError(errors, $"{prefix}.Quantity", $"{label}: the line amount is too large.");
                continue;
            }

            if (line.DiscountAmount > gross)
            {
                AddError(errors, $"{prefix}.DiscountAmount", $"{label}: the discount cannot be more than the line amount.");
                continue;
            }

            var net = gross - line.DiscountAmount;
            var lineTax = Money(net * pick.TaxRate / 100m);

            line.UnitPrice = unitPrice;
            line.TaxAmount = lineTax;
            line.LineTotal = net + lineTax;

            subTotal += gross;
            discountTotal += line.DiscountAmount;
            taxTotal += lineTax;
        }

        input.SubTotal = Money(subTotal);
        input.DiscountAmount = Money(discountTotal);
        input.TaxAmount = Money(taxTotal);
        input.TotalAmount = Money(input.SubTotal - input.DiscountAmount + input.TaxAmount);

        if (input.TotalAmount > MaxAmount)
            AddError(errors, "Lines", "The sale total is too large.");
    }

    private static void PreparePayments(InputSaleCreate input, Dictionary<string, List<string>> errors)
    {
        if (input.Payments.Count == 0)
        {
            AddError(errors, "Payments", "Add at least one payment.");
            return;
        }

        decimal paid = 0;
        decimal cashPaid = 0;

        for (var i = 0; i < input.Payments.Count; i++)
        {
            var pay = input.Payments[i];
            var prefix = $"Payments[{i}]";
            var label = $"Payment {i + 1}";

            pay.PaymentMethod = (pay.PaymentMethod ?? string.Empty).Trim().ToLowerInvariant();
            pay.Amount = Money(pay.Amount);
            pay.ReferenceNo = Clean(pay.ReferenceNo);

            if (Array.IndexOf(PaymentMethods, pay.PaymentMethod) < 0)
            {
                AddError(errors, $"{prefix}.PaymentMethod", $"{label}: choose a valid payment method.");
                continue;
            }

            if (pay.Amount <= 0)
            {
                AddError(errors, $"{prefix}.Amount", $"{label}: the amount must be greater than zero.");
                continue;
            }

            paid += pay.Amount;
            if (pay.PaymentMethod == "cash")
                cashPaid += pay.Amount;
        }

        input.PaidAmount = Money(paid);
        if (input.PaidAmount + 0.0001m < input.TotalAmount)
        {
            AddError(errors, "Payments",
                $"The amount paid ({FormatMoney(input.PaidAmount)}) is less than the total ({FormatMoney(input.TotalAmount)}).");
            return;
        }

        var change = Money(Math.Max(0, input.PaidAmount - input.TotalAmount));
        if (change > 0 && cashPaid + 0.0001m < change)
        {
            AddError(errors, "Payments", "Change can only be given from a cash payment.");
            return;
        }

        input.ChangeGiven = change;
    }

    private async Task ApplyDefaultTaxAsync(int fkStore, IEnumerable<OutputSaleProductSelect> items, CancellationToken ct)
    {
        var defaultRate = await ResolveDefaultTaxRateAsync(fkStore, ct);
        foreach (var item in items)
        {
            if (!item.HasProductTax)
                item.TaxRate = defaultRate;
        }
    }

    private async Task<decimal> ResolveDefaultTaxRateAsync(int fkStore, CancellationToken ct)
    {
        var code = await _settings.GetEffectiveValueAsync("default_tax_code", fkStore, ct);
        if (string.IsNullOrWhiteSpace(code))
            return 0;

        return Money(await _repo.GetTaxRateByCodeAsync(code, ct));
    }

    private async Task ApplyDateBoundsAsync(
        int fkStore,
        DateTime? from,
        DateTime? to,
        Action<DateTime?, DateTime?> assign,
        CancellationToken ct)
    {
        if (from.HasValue && to.HasValue && from > to)
            (from, to) = (to, from);

        if (!from.HasValue && !to.HasValue)
        {
            assign(null, null);
            return;
        }

        var tz = await ResolveTimeZoneAsync(fkStore, ct);
        DateTime? fromUtc = from.HasValue ? StartOfLocalDayUtc(from.Value, tz) : null;
        DateTime? toExclusive = to.HasValue ? StartOfLocalDayUtc(to.Value.Date.AddDays(1), tz) : null;
        assign(fromUtc, toExclusive);
    }

    private async Task<TimeZoneInfo> ResolveTimeZoneAsync(int fkStore, CancellationToken ct)
    {
        var id = await _settings.GetEffectiveValueAsync("locale.timezone", fkStore, ct);
        if (string.IsNullOrWhiteSpace(id))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTime StartOfLocalDayUtc(DateTime value, TimeZoneInfo tz)
    {
        var local = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, tz);
    }

    private static void ApplyInvoiceRules(OutputSaleSelect sale)
    {
        var open = !sale.Cancelled && sale.Status == "completed";
        sale.CanCancel = open && !sale.HasReturns;
        sale.CanReturn = open;
    }

    private static void RequireStore(int fkStore, string userMessage)
    {
        if (fkStore <= 0)
            throw new ValidationException(userMessage, "fk_store is 0: platform accounts are not tied to a store.");
    }

    private static SemaphoreSlim GateFor(int key) => Gates[(int)((uint)key % (uint)Gates.Length)];

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
            errors[key] = list = new List<string>();
        list.Add(message);
    }

    private static void ThrowIfInvalid(Dictionary<string, List<string>> errors)
    {
        if (errors.Count == 0) return;

        var fieldErrors = errors.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().ToArray());
        var all = fieldErrors.Values.SelectMany(v => v).Distinct().ToList();
        var summary = string.Join(" ", all.Take(3)) + (all.Count > 3 ? $" (and {all.Count - 3} more)" : string.Empty);
        throw new ValidationException(summary, summary, fieldErrors);
    }

    private static AppException Failure(int statusCode, string? message, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        var developer = $"Sale repository returned {statusCode}: {text}";
        return statusCode switch
        {
            404 => new ResourceNotFoundException(text, developer),
            409 => new ConflictException(text, developer),
            422 => new ValidationException(text, developer),
            _ => new UpdateFailedException(text, developer)
        };
    }

    private static string? StripClientRef(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        var text = notes.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!text.StartsWith("__ref:", StringComparison.Ordinal))
            return notes.Trim();

        var nl = text.IndexOf('\n');
        if (nl < 0)
            return null;

        var rest = text[(nl + 1)..].Trim();
        return rest.Length == 0 ? null : rest;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string FormatMoney(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
}

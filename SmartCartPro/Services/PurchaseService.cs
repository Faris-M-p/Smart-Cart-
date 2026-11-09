using System.Globalization;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class PurchaseService : IPurchaseService
{
    private const int MaxLines = 200;
    private const decimal MaxQuantity = 9_999_999.999m;
    private const decimal MaxUnitCost = 9_999_999.99m;
    private const decimal MaxAmount = 9_999_999_999.99m;

    private static readonly int[] PageSizes = { 10, 25, 50, 100 };
    private static readonly string[] Statuses = { "pending", "approved", "cancelled" };
    private static readonly string[] SortFields = { "id", "purchasedate", "supplier", "invoicenumber", "totalamount", "status" };

    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private readonly IPurchaseRepository _repo;

    public PurchaseService(IPurchaseRepository repo)
    {
        _repo = repo;
    }

    #region Purchases

    public async Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);
        return await _repo.GetPurchaseAsync(input, ct);
    }

    public async Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default)
    {
        input.Search = Clean(input.Search);
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];

        var status = (input.Status ?? string.Empty).Trim().ToLowerInvariant();
        input.Status = Array.IndexOf(Statuses, status) >= 0 ? status : null;

        if (input.FkSupplier is <= 0) input.FkSupplier = null;
        if (input.From.HasValue && input.To.HasValue && input.From > input.To)
            (input.From, input.To) = (input.To, input.From);

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(SortFields, sort) >= 0 ? sort : "purchasedate";

        var result = await _repo.ListPurchasesAsync(input, ct);

        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListPurchasesAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);

        var errors = new Dictionary<string, List<string>>();
        ValidateHeader(input.PurchaseDate, "PurchaseDate", "Purchase Date", errors);
        if (input.InvoiceDate != default)
            ValidateHeader(input.InvoiceDate, "InvoiceDate", "Invoice Date", errors);

        var (subTotal, discountTotal, taxTotal, total, taxableTotal) = await PrepareLinesAsync(input.Lines, errors, ct);
        ThrowIfInvalid(errors);

        input.InvoiceNumber = Clean(input.InvoiceNumber);
        input.Notes = Clean(input.Notes);
        input.SubTotal = subTotal;
        input.DiscountAmount = discountTotal;
        input.TaxableAmount = taxableTotal;
        input.TaxAmount = taxTotal;
        input.TotalAmount = total + input.Charges + input.RoundOff;

        return await _repo.CreatePurchaseAsync(input, ct);
    }

    public async Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);

        var errors = new Dictionary<string, List<string>>();
        ValidateHeader(input.PurchaseDate, "PurchaseDate", "Purchase Date", errors);
        if (input.InvoiceDate != default)
            ValidateHeader(input.InvoiceDate, "InvoiceDate", "Invoice Date", errors);

        var (subTotal, discountTotal, taxTotal, total, taxableTotal) = await PrepareLinesAsync(input.Lines, errors, ct);
        ThrowIfInvalid(errors);

        input.InvoiceNumber = Clean(input.InvoiceNumber);
        input.Notes = Clean(input.Notes);
        input.SubTotal = subTotal;
        input.DiscountAmount = discountTotal;
        input.TaxableAmount = taxableTotal;
        input.TaxAmount = taxTotal;
        input.TotalAmount = total + input.Charges + input.RoundOff;

        var gate = GateFor(input.IdPurchase);
        await gate.WaitAsync(ct);
        try
        {
            return await _repo.UpdatePurchaseAsync(input, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task ApprovePurchaseAsync(int id, int storeId, int userId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        var gate = GateFor(id);
        await gate.WaitAsync(ct);
        try
        {
            await _repo.ApprovePurchaseAsync(id, storeId, userId, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);

        input.Reason = (input.Reason ?? string.Empty).Trim();
        if (input.Reason.Length < 2)
        {
            const string message = "Please enter a reason for cancelling.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["Reason"] = new[] { message } });
        }

        var gate = GateFor(input.IdPurchase);
        await gate.WaitAsync(ct);
        try
        {
            return await _repo.CancelPurchaseAsync(input, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<IReadOnlyList<OutputPurchaseProductSelect>> ListProductsAsync(InputPurchaseProductList input, CancellationToken ct = default)
    {
        var term = Clean(input.Search);
        if (term is null || term.Length < 2)
        {
            if (input.ProductIds == null || input.ProductIds.Count == 0)
                return Array.Empty<OutputPurchaseProductSelect>();
        }

        if (term != null)
            input.Search = term.Length > 100 ? term[..100] : term;
        input.Limit = input.Limit is > 0 and <= 50 ? input.Limit : 10;

        return await _repo.ListProductsAsync(input, ct);
    }

    #endregion

    #region Purchase Returns

    public async Task<OutputPurchaseReturnSelect?> GetReturnAsync(int id, int storeId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        return await _repo.GetReturnAsync(id, storeId, ct);
    }

    public async Task<OutputPurchaseReturnList> ListReturnsAsync(InputPurchaseReturnList input, CancellationToken ct = default)
    {
        if (input.FkStore is not null && input.FkStore > 0)
            RequireStore(input.FkStore.Value);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);
        return await _repo.ListReturnsAsync(input, ct);
    }

    public async Task<int> CreateReturnAsync(InPurchaseReturnCreateView input, int storeId, int userId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (input.FkPurchase <= 0)
            throw new ValidationException("FkPurchase", "A valid purchase reference is required.");
        if (input.Lines == null || input.Lines.Count == 0)
            throw new ValidationException("Lines", "At least one item must be returned.");

        foreach (var line in input.Lines)
        {
            if (line.Quantity <= 0)
                throw new ValidationException("Lines", "Return quantity must be greater than zero.");
        }

        return await _repo.CreateReturnAsync(input, storeId, userId, ct);
    }

    public async Task ApproveReturnAsync(int id, int storeId, int userId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        var gate = GateFor(id);
        await gate.WaitAsync(ct);
        try
        {
            await _repo.ApproveReturnAsync(id, storeId, userId, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task CancelReturnAsync(int id, int storeId, string reason, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 2)
            throw new ValidationException("Reason", "Please enter a valid cancellation reason.");

        var gate = GateFor(id);
        await gate.WaitAsync(ct);
        try
        {
            await _repo.CancelReturnAsync(id, storeId, reason.Trim(), ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<IReadOnlyList<OutputPurchaseListItem>> ListReturnSourcesAsync(int storeId, string? search = null, CancellationToken ct = default)
    {
        RequireStore(storeId);
        return await _repo.ListReturnSourcePurchasesAsync(storeId, search, ct);
    }

    #endregion

    #region Purchase Orders

    public async Task<OutputPurchaseOrderSelect?> GetOrderAsync(int id, int storeId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        return await _repo.GetOrderAsync(id, storeId, ct);
    }

    public async Task<OutputPurchaseOrderList> ListOrdersAsync(InputPurchaseOrderList input, CancellationToken ct = default)
    {
        if (input.FkStore is not null && input.FkStore > 0)
            RequireStore(input.FkStore.Value);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);
        return await _repo.ListOrdersAsync(input, ct);
    }

    public async Task<int> SaveOrderAsync(InPurchaseOrderView input, int storeId, int userId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (input.Lines == null || input.Lines.Count == 0)
            throw new ValidationException("Lines", "At least one requirement line is required.");

        foreach (var l in input.Lines)
        {
            if (l.FkProduct <= 0)
                throw new ValidationException("Lines", "Please select a valid product for each line.");
            if (l.Quantity <= 0)
                throw new ValidationException("Lines", "Quantity must be greater than zero.");
        }

        return await _repo.SaveOrderAsync(input, storeId, userId, ct);
    }

    public async Task DeleteOrderAsync(int id, int storeId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        await _repo.DeleteOrderAsync(id, storeId, ct);
    }

    #endregion

    #region Supplier Financials & Balance

    public async Task<OutputSupplierBalance> GetSupplierBalanceAsync(int supplierId, int storeId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (supplierId <= 0)
            throw new ValidationException("SupplierId", "Invalid supplier ID.");
        return await _repo.GetSupplierBalanceAsync(supplierId, storeId, ct);
    }

    public async Task<OutputSupplierStatement> GetSupplierStatementAsync(int supplierId, int storeId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (supplierId <= 0)
            throw new ValidationException("SupplierId", "Invalid supplier ID.");
        return await _repo.GetSupplierStatementAsync(supplierId, storeId, from, to, ct);
    }

    public async Task<OutputSupplierTransactionList> ListSupplierTransactionsAsync(InputSupplierTransactionList input, CancellationToken ct = default)
    {
        if (input.FkStore is not null && input.FkStore > 0)
            RequireStore(input.FkStore.Value);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);
        return await _repo.ListSupplierTransactionsAsync(input, ct);
    }

    public async Task<int> RecordSupplierTransactionAsync(InSupplierPaymentView input, int storeId, int userId, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (input.FkSupplier <= 0)
            throw new ValidationException("FkSupplier", "Please select a valid supplier.");
        if (input.Amount <= 0)
            throw new ValidationException("Amount", "Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(input.TransactionType))
            input.TransactionType = "Payment";
        if (string.IsNullOrWhiteSpace(input.PaymentMethod))
            input.PaymentMethod = "Cash";

        return await _repo.RecordSupplierTransactionAsync(input, storeId, userId, ct);
    }

    public async Task ReverseSupplierTransactionAsync(int id, int storeId, int userId, string reason, CancellationToken ct = default)
    {
        RequireStore(storeId);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 2)
            throw new ValidationException("Reason", "Please enter a valid reversal reason.");

        var gate = GateFor(id);
        await gate.WaitAsync(ct);
        try
        {
            await _repo.ReverseSupplierTransactionAsync(id, storeId, userId, reason.Trim(), ct);
        }
        finally
        {
            gate.Release();
        }
    }

    #endregion

    #region Private Helpers

    private async Task<(decimal SubTotal, decimal Discount, decimal Tax, decimal Total, decimal Taxable)> PrepareLinesAsync(
        List<InputPurchaseLine> lines,
        Dictionary<string, List<string>> errors,
        CancellationToken ct)
    {
        if (lines.Count == 0)
        {
            AddError(errors, "Lines", "At least one purchase line is required.");
            return default;
        }

        if (lines.Count > MaxLines)
        {
            AddError(errors, "Lines", $"A purchase can have at most {MaxLines} lines.");
            return default;
        }

        foreach (var line in lines)
            line.FkProductVariant = line.FkProductVariant is > 0 ? line.FkProductVariant : null;

        var productIds = lines.Select(l => l.FkProduct).Where(id => id > 0).Distinct().ToList();
        var picks = productIds.Count == 0
            ? new List<OutputPurchaseProductSelect>()
            : (await _repo.ListProductsAsync(new InputPurchaseProductList { ProductIds = productIds, Limit = productIds.Count }, ct)).ToList();
        var byKey = picks.ToDictionary(p => (p.FkProduct, p.FkProductVariant ?? 0));

        decimal subTotal = 0m, discountTotal = 0m, taxTotal = 0m, taxableTotal = 0m;
        var firstLineOf = new Dictionary<(int, int), int>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var prefix = $"Lines[{i}]";
            var label = $"Line {i + 1}";

            line.Quantity = Math.Round(line.Quantity, 3, MidpointRounding.AwayFromZero);
            line.UnitCost = Math.Round(line.UnitCost, 2, MidpointRounding.AwayFromZero);
            line.DiscountAmount = Math.Round(line.DiscountAmount, 2, MidpointRounding.AwayFromZero);

            var key = (line.FkProduct, line.FkProductVariant ?? 0);
            if (!byKey.TryGetValue(key, out var pick))
            {
                AddError(errors, $"{prefix}.FkProduct", $"{label}: the product is not available. Remove the line or pick another product.");
                continue;
            }

            if (firstLineOf.TryGetValue(key, out var firstLine))
                AddError(errors, $"{prefix}.FkProduct", $"{label}: this product is already on line {firstLine}. Combine the quantities into one line.");
            else
                firstLineOf[key] = i + 1;

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

            if (line.UnitCost < 0)
            {
                AddError(errors, $"{prefix}.UnitCost", $"{label}: the unit cost cannot be negative.");
                valid = false;
            }
            else if (line.UnitCost > MaxUnitCost)
            {
                AddError(errors, $"{prefix}.UnitCost", $"{label}: the unit cost is too large.");
                valid = false;
            }

            if (line.DiscountAmount < 0)
            {
                AddError(errors, $"{prefix}.DiscountAmount", $"{label}: the discount cannot be negative.");
                valid = false;
            }

            if (!valid) continue;

            var gross = Math.Round(line.Quantity * line.UnitCost, 2, MidpointRounding.AwayFromZero);
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
            var taxRate = line.TaxRate > 0 ? line.TaxRate : pick.TaxRate;
            line.TaxRate = taxRate;

            decimal lineTax;
            decimal taxable;
            if (line.TaxInclusive)
            {
                taxable = Math.Round(net / (1m + (taxRate / 100m)), 2, MidpointRounding.AwayFromZero);
                lineTax = net - taxable;
            }
            else
            {
                taxable = net;
                lineTax = Math.Round(taxable * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);
            }

            line.TaxableAmount = taxable;
            line.TaxAmount = lineTax;
            line.LineTotal = taxable + lineTax;

            subTotal += gross;
            discountTotal += line.DiscountAmount;
            taxableTotal += taxable;
            taxTotal += lineTax;
        }

        var total = subTotal - discountTotal + taxTotal;
        if (subTotal > MaxAmount || total > MaxAmount)
            AddError(errors, "Lines", "The purchase total is too large.");

        return (subTotal, discountTotal, taxTotal, total, taxableTotal);
    }

    private static void ValidateHeader(DateOnly date, string field, string label, Dictionary<string, List<string>> errors)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date < new DateOnly(2000, 1, 1) || date > today.AddDays(1))
            AddError(errors, field, $"The {label} is not valid.");
    }

    private static void RequireStore(int fkStore)
    {
        if (fkStore <= 0)
            throw new ForbiddenException(
                "Sign in as a store user to work with purchases.",
                "fk_store is 0: platform accounts are not tied to a store.");
    }

    private static SemaphoreSlim GateFor(int id) => Gates[(int)((uint)id % (uint)Gates.Length)];

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

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion
}

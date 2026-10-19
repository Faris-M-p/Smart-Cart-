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
    private static readonly string[] Statuses = { "draft", "posted", "partially_received", "received", "cancelled" };
    private static readonly string[] SortFields = { "id", "purchasedate", "supplier", "invoicenumber", "totalamount", "status" };

    // In-process gates: edit / receive / cancel of the same purchase order never run side by side in this instance.
    // (A multi-instance deployment would need a database lock or a concurrency token on the purchase row.)
    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private readonly IPurchaseRepository _repo;
    private readonly IDocumentNumberService _docNum;

    public PurchaseService(IPurchaseRepository repo, IDocumentNumberService docNum)
    {
        _repo = repo;
        _docNum = docNum;
    }

    public async Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default)
    {
        var po = await _repo.GetPurchaseAsync(input, ct);
        if (po is not null) ApplyRules(po);
        return po;
    }

    public async Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default)
    {
        input.Search = Clean(input.Search);
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];
        input.SearchId = ParsePoNumber(input.Search);

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

        // A stale link can point past the last page — show the last page instead of an empty one.
        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListPurchasesAsync(input, ct);
        }

        foreach (var item in result.Items)
        {
            item.CanEdit = item.Status is "draft" or "posted";
            item.CanReceive = item.Status is "posted" or "partially_received";
            item.CanCancel = item.Status is "draft" or "posted" or "partially_received";
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);

        var errors = new Dictionary<string, List<string>>();
        ValidateHeader(input.PurchaseDate, "PurchaseDate", "Purchase Date", errors);
        var totals = await PrepareLinesAsync(input.Lines, errors, ct);
        ThrowIfInvalid(errors);

        input.InvoiceNumber = Clean(input.InvoiceNumber);
        input.Notes = Clean(input.Notes);
        input.SubTotal = totals.SubTotal;
        input.DiscountAmount = totals.Discount;
        input.TaxAmount = totals.Tax;
        input.TotalAmount = totals.Total;

        var result = await _repo.CreatePurchaseAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to create the purchase order.", "FkSupplier");

        return result;
    }

    public async Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);

        var errors = new Dictionary<string, List<string>>();
        ValidateHeader(input.PurchaseDate, "PurchaseDate", "Purchase Date", errors);
        var totals = await PrepareLinesAsync(input.Lines, errors, ct);
        ThrowIfInvalid(errors);

        input.InvoiceNumber = Clean(input.InvoiceNumber);
        input.Notes = Clean(input.Notes);
        input.SubTotal = totals.SubTotal;
        input.DiscountAmount = totals.Discount;
        input.TaxAmount = totals.Tax;
        input.TotalAmount = totals.Total;

        var gate = GateFor(input.IdPurchase);
        await gate.WaitAsync(ct);
        try
        {
            var result = await _repo.UpdatePurchaseAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update the purchase order.", "FkSupplier");

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default)
    {
        RequireStore(input.FkStore);

        var gate = GateFor(input.IdPurchase);
        await gate.WaitAsync(ct);
        try
        {
            var select = new InputPurchaseSelect { IdPurchase = input.IdPurchase, FkStore = input.FkStore };
            var po = await GetPurchaseAsync(select, ct);
            if (po is null)
                throw new ResourceNotFoundException("Purchase order not found.", $"Purchase {input.IdPurchase} was not found in store {input.FkStore}.");

            if (!po.CanReceive)
                throw new ConflictException(
                    "Goods can only be received against a posted or partially received purchase order that still has pending quantities.",
                    $"Purchase {po.IdPurchase} has status '{po.Status}' (cancelled: {po.Cancelled}).");

            var errors = new Dictionary<string, List<string>>();
            ValidateHeader(input.ReceiptDate, "ReceiptDate", "Receipt Date", errors);

            var byDetail = po.Lines.ToDictionary(l => l.IdPurchaseDetail);
            var seen = new HashSet<int>();
            var resolved = new List<InputGrnLine>();

            for (var i = 0; i < input.Lines.Count; i++)
            {
                var line = input.Lines[i];
                var prefix = $"Lines[{i}]";
                var label = $"Line {i + 1}";

                if (!byDetail.TryGetValue(line.IdPurchaseDetail, out var poLine))
                {
                    AddError(errors, $"{prefix}.IdPurchaseDetail", $"{label}: this line does not belong to the purchase order.");
                    continue;
                }

                if (!seen.Add(line.IdPurchaseDetail))
                {
                    AddError(errors, $"{prefix}.IdPurchaseDetail", $"{label}: this line is listed more than once.");
                    continue;
                }

                var qty = Math.Round(line.ReceiveQty, 3, MidpointRounding.AwayFromZero);
                if (qty < 0)
                {
                    AddError(errors, $"{prefix}.ReceiveQty", $"{label}: the receive quantity cannot be negative.");
                    continue;
                }

                if (qty == 0) continue;

                if (qty > poLine.PendingQty)
                {
                    AddError(errors, $"{prefix}.ReceiveQty", $"{label}: only {Format(poLine.PendingQty)} is still pending (you entered {Format(qty)}).");
                    continue;
                }

                // Product and variant always come from the purchase order line, never from the client.
                resolved.Add(new InputGrnLine
                {
                    IdPurchaseDetail = poLine.IdPurchaseDetail,
                    ReceiveQty = qty,
                    FkProduct = poLine.FkProduct,
                    FkProductVariant = poLine.FkProductVariant
                });
            }

            if (resolved.Count == 0 && errors.Count == 0)
                AddError(errors, "Lines", "Enter a receive quantity for at least one line.");

            ThrowIfInvalid(errors);

            var receivedNow = resolved.ToDictionary(r => r.IdPurchaseDetail, r => r.ReceiveQty);
            var allReceived = po.Lines.All(l =>
                l.ReceivedQty + (receivedNow.TryGetValue(l.IdPurchaseDetail, out var now) ? now : 0m) >= l.Quantity);

            input.Lines = resolved;
            input.Notes = Clean(input.Notes);
            input.NewStatus = allReceived ? "received" : "partially_received";
            input.GrnNumber = await _docNum.NextAsync(input.FkStore, "GRN", ct);

            var result = await _repo.ReceiveGoodsAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to record the goods receipt.");

            return result;
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
            var result = await _repo.CancelPurchaseAsync(input, ct);
            if (!result.Ok)
                throw Failure(result.StatusCode, result.ResponseMsg, "Unable to cancel the purchase order.");

            return result;
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
            return Array.Empty<OutputPurchaseProductSelect>();

        input.Search = term.Length > 100 ? term[..100] : term;
        input.ProductIds = null;
        input.Limit = input.Limit is > 0 and <= 20 ? input.Limit : 10;

        return await _repo.ListProductsAsync(input, ct);
    }

    // ───────────────────────── helpers ─────────────────────────

    /// <summary>Which actions are allowed on a purchase order — the single source of truth for views and guards.</summary>
    private static void ApplyRules(OutputPurchaseSelect po)
    {
        var open = !po.Cancelled && po.Status != "cancelled";

        // Normally identical to the stored status. If a goods receipt was interrupted after its stock movements were
        // saved, the received quantities (derived from stock movements) are the truth — show them instead of a stale status.
        if (open && (po.Status is "posted" or "partially_received") && po.Lines.Count > 0)
        {
            if (po.Lines.All(l => l.PendingQty <= 0)) po.Status = "received";
            else if (po.Lines.Any(l => l.ReceivedQty > 0)) po.Status = "partially_received";
        }

        var hasReceipts = po.Lines.Any(l => l.ReceivedQty > 0);
        var hasPending = po.Lines.Any(l => l.PendingQty > 0);

        po.CanEdit = open && !hasReceipts && (po.Status is "draft" or "posted");
        po.CanReceive = open && hasPending && (po.Status is "posted" or "partially_received");
        po.CanCancel = open && po.Status != "received";
    }

    /// <summary>
    /// Validates the lines against master data and recomputes every amount: product must be active, tax comes from the
    /// product's tax profile, discount cannot exceed the line amount. Client-side totals are never trusted.
    /// </summary>
    private async Task<(decimal SubTotal, decimal Discount, decimal Tax, decimal Total)> PrepareLinesAsync(
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
            AddError(errors, "Lines", $"A purchase order can have at most {MaxLines} lines.");
            return default;
        }

        foreach (var line in lines)
            line.FkProductVariant = line.FkProductVariant is > 0 ? line.FkProductVariant : null;

        var productIds = lines.Select(l => l.FkProduct).Where(id => id > 0).Distinct().ToList();
        var picks = productIds.Count == 0
            ? new List<OutputPurchaseProductSelect>()
            : (await _repo.ListProductsAsync(new InputPurchaseProductList { ProductIds = productIds, Limit = productIds.Count }, ct)).ToList();
        var byKey = picks.ToDictionary(p => (p.FkProduct, p.FkProductVariant ?? 0));

        decimal subTotal = 0m, discountTotal = 0m, taxTotal = 0m;
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

            // Tax is calculated on the discounted amount (tax-exclusive), the same way sales are taxed.
            var net = gross - line.DiscountAmount;
            var lineTax = Math.Round(net * pick.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);

            line.TaxAmount = lineTax;
            line.LineTotal = net + lineTax;

            subTotal += gross;
            discountTotal += line.DiscountAmount;
            taxTotal += lineTax;
        }

        var total = subTotal - discountTotal + taxTotal;
        if (subTotal > MaxAmount || total > MaxAmount)
            AddError(errors, "Lines", "The purchase total is too large.");

        return (subTotal, discountTotal, taxTotal, total);
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

    private static SemaphoreSlim GateFor(int idPurchase) => Gates[(int)((uint)idPurchase % (uint)Gates.Length)];

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

    private static AppException Failure(int statusCode, string? message, string fallback, string? field = null)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        var developer = $"Purchase repository returned {statusCode}: {text}";

        return statusCode switch
        {
            404 => new ResourceNotFoundException(text, developer),
            409 => new ConflictException(text, developer),
            422 => new ValidationException(text, developer,
                field is null ? null : new Dictionary<string, string[]> { [field] = new[] { text } }),
            _ => new UpdateFailedException(text, developer)
        };
    }

    private static int? ParsePoNumber(string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return null;

        var text = search.Trim();
        if (text.StartsWith("PO", StringComparison.OrdinalIgnoreCase))
            text = text[2..].TrimStart('-', ' ');

        return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : null;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Format(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

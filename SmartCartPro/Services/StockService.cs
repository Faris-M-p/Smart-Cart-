using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class StockService : IStockService
{
    private const int MaxTransferLines = 100;

    private readonly IStockRepository _repo;

    public StockService(IStockRepository repo) => _repo = repo;

    // ═══════════════════════════════════════════════════════════════
    // OPENING STOCK
    // ═══════════════════════════════════════════════════════════════

    public Task<OutputStockMovement> ApplyOpeningStockAsync(InOpeningStockView view, int userId, CancellationToken ct = default)
    {
        var input = new InputStockOpening
        {
            FkStore = view.FkStore,
            FkProduct = view.FkProduct,
            FkProductVariant = (view.FkProductVariant is > 0) ? view.FkProductVariant : null,
            Quantity = view.Quantity,
            Notes = view.Notes,
            CreatedBy = userId
        };
        return ApplyOpeningStockAsync(input, ct);
    }

    public async Task<OutputStockMovement> ApplyOpeningStockAsync(InputStockOpening input, CancellationToken ct = default)
    {
        if (input.FkStore <= 0)
            return Failed(422, "Sign in as a store user to record opening stock.");

        if (input.Quantity <= 0)
            return Failed(422, "The opening quantity must be greater than zero.");

        var select = new InputInventorySelect
        {
            FkStore = input.FkStore,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant
        };

        var sku = await _repo.GetStockLevelAsync(select, ct);
        if (sku is null)
            return Failed(404, "The selected product was not found. Search again and pick it from the list.");

        // Opening stock is the very first entry for a SKU. Later corrections are adjustments, so a second
        // "opening" can never silently stack on top of the first.
        if (await _repo.HasOpeningStockAsync(select, ct))
            return Failed(409, $"Opening stock was already recorded for {Describe(sku)} at this store. Use Stock adjustment to correct the quantity.");

        var movement = new InputStockMovement
        {
            FkStore = input.FkStore,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant,
            MovementType = "opening",
            Quantity = input.Quantity,
            Notes = Clean(input.Notes),
            CreatedBy = input.CreatedBy
        };

        return await _repo.ApplyMovementAsync(movement, ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // ADJUSTMENT
    // ═══════════════════════════════════════════════════════════════

    public Task<OutputStockMovement> AdjustStockAsync(InStockAdjustView view, int userId, CancellationToken ct = default)
    {
        var input = new InputStockAdjust
        {
            FkStore = view.FkStore,
            FkProduct = view.FkProduct,
            FkProductVariant = (view.FkProductVariant is > 0) ? view.FkProductVariant : null,
            Quantity = view.Quantity,
            Direction = view.Direction,
            Reason = view.Reason,
            Notes = view.Notes,
            CreatedBy = userId
        };
        return AdjustStockAsync(input, ct);
    }

    public async Task<OutputStockMovement> AdjustStockAsync(InputStockAdjust input, CancellationToken ct = default)
    {
        if (input.FkStore <= 0)
            return Failed(422, "Sign in as a store user to adjust stock.");

        var quantity = Math.Abs(input.Quantity);
        if (quantity <= 0)
            return Failed(422, "The quantity must be greater than zero.");

        var isAdd = string.Equals(input.Direction, "add", StringComparison.OrdinalIgnoreCase);
        var isRemove = string.Equals(input.Direction, "remove", StringComparison.OrdinalIgnoreCase);
        if (!isAdd && !isRemove)
            return Failed(422, "Choose whether to add or remove stock.");

        var select = new InputInventorySelect
        {
            FkStore = input.FkStore,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant
        };

        var sku = await _repo.GetStockLevelAsync(select, ct);
        if (sku is null)
            return Failed(404, "The selected product was not found. Search again and pick it from the list.");

        // "Damaged: two cartons dropped" — the reason always leads, the free text follows.
        var reason = Clean(input.Reason) ?? "Adjustment";
        var note = Clean(input.Notes);

        var movement = new InputStockMovement
        {
            FkStore = input.FkStore,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant,
            MovementType = "adjustment",
            // "adjustment" is the one signed movement type: positive adds stock, negative removes it.
            // The repository refuses a removal larger than the quantity on hand.
            Quantity = isAdd ? quantity : -quantity,
            Notes = note is null ? reason : $"{reason}: {note}",
            CreatedBy = input.CreatedBy
        };

        return await _repo.ApplyMovementAsync(movement, ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // TRANSFER
    // ═══════════════════════════════════════════════════════════════

    public Task<OutputStockTransfer> TransferStockAsync(InputStockTransfer input, CancellationToken ct = default)
        => _repo.TransferAsync(input, ct);

    public async Task<OutputStockTransferCreate> CreateStockTransferAsync(InputStockTransferCreate input, CancellationToken ct = default)
    {
        if (input.FromStoreId <= 0)
            return TransferFailed(422, "Sign in as a store user to transfer stock.");

        if (input.ToStoreId <= 0 || input.ToStoreId == input.FromStoreId)
            return TransferFailed(422, "Choose a different destination store.");

        if (input.Lines.Count == 0)
            return TransferFailed(422, "Add at least one product to transfer.");

        if (input.Lines.Count > MaxTransferLines)
            return TransferFailed(422, $"A transfer can have at most {MaxTransferLines} lines.");

        // The same SKU on several lines becomes one combined line: one stock check, one movement pair.
        var merged = new List<InputStockTransferLine>();
        foreach (var line in input.Lines)
        {
            if (line.Quantity <= 0)
                return TransferFailed(422, "The quantity must be greater than zero on every line.");

            var existing = merged.FirstOrDefault(m => m.FkProduct == line.FkProduct && m.FkProductVariant == line.FkProductVariant);
            if (existing is null)
            {
                merged.Add(new InputStockTransferLine
                {
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.Quantity
                });
            }
            else
            {
                existing.Quantity += line.Quantity;
            }
        }

        foreach (var line in merged)
        {
            var select = new InputInventorySelect
            {
                FkStore = input.FromStoreId,
                FkProduct = line.FkProduct,
                FkProductVariant = line.FkProductVariant
            };

            var sku = await _repo.GetStockLevelAsync(select, ct);
            if (sku is null)
                return TransferFailed(404, "A selected product was not found. Remove it from the list and search again.");
        }

        var create = new InputStockTransferCreate
        {
            FromStoreId = input.FromStoreId,
            ToStoreId = input.ToStoreId,
            Notes = Clean(input.Notes),
            CreatedBy = input.CreatedBy,
            Lines = merged
        };

        return await _repo.CreateTransferAsync(create, ct);
    }

    public Task<List<OutputStockLookupItem>> GetTransferStoresAsync(int fromStoreId, CancellationToken ct = default)
        => _repo.ListTransferStoresAsync(fromStoreId, ct);

    // ═══════════════════════════════════════════════════════════════
    // READS
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default)
    {
        var result = await _repo.ListInventoryAsync(input, ct);
        result.Categories = await _repo.ListCategoryOptionsAsync(ct);
        return result;
    }

    public Task<OutputStockMovementList> GetMovementHistoryAsync(InputStockMovementList input, CancellationToken ct = default)
    {
        input.From = SaneDay(input.From);
        input.To = SaneDay(input.To);

        // A reversed range is almost certainly a slip: swap instead of showing an empty list.
        if (input.From.HasValue && input.To.HasValue && input.From.Value > input.To.Value)
            (input.From, input.To) = (input.To, input.From);

        // The day filters are the user's LOCAL days; timestamps are stored in UTC.
        // JavaScript getTimezoneOffset() = UTC − local, so local midnight + offset = that same instant in UTC.
        var offset = Math.Clamp(input.TzOffsetMinutes, -840, 840);
        input.TzOffsetMinutes = offset;
        input.FromUtc = input.From.HasValue ? input.From.Value.AddMinutes(offset) : null;
        input.ToUtcExclusive = input.To.HasValue ? input.To.Value.AddDays(1).AddMinutes(offset) : null;

        return _repo.ListMovementsAsync(input, ct);
    }

    public async Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default)
    {
        var result = await _repo.GetLowStockAsync(input, ct);
        result.Categories = await _repo.ListCategoryOptionsAsync(ct);

        // Suggested order: refill up to twice the reorder level. Blank (0) when the SKU has no reorder level set.
        foreach (var item in result.Items)
        {
            item.SuggestedOrderQty = item.ReorderLevel > 0
                ? Math.Max(0m, Math.Ceiling((item.ReorderLevel * 2) - item.QuantityOnHand))
                : 0m;
        }

        return result;
    }

    public Task<OutputInventorySelect?> GetStockLevelAsync(InputInventorySelect input, CancellationToken ct = default)
        => _repo.GetStockLevelAsync(input, ct);

    public Task<OutputStockProductSearch> SearchProductsAsync(InputStockProductSearch input, CancellationToken ct = default)
    {
        input.Term = Clean(input.Term);
        return _repo.SearchProductsAsync(input, ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Describe(OutputInventorySelect sku)
        => string.IsNullOrWhiteSpace(sku.VariantName) ? sku.ProductName : $"{sku.ProductName} — {sku.VariantName}";

    /// <summary>Ignores absurd dates typed into the URL (they would overflow the day arithmetic).</summary>
    private static DateTime? SaneDay(DateTime? value)
        => (value.HasValue && value.Value.Year >= 1900 && value.Value.Year <= 2200) ? value.Value.Date : null;

    private static OutputStockMovement Failed(int statusCode, string message)
        => new() { ResponseCode = 1, StatusCode = statusCode, ResponseMsg = message };

    private static OutputStockTransferCreate TransferFailed(int statusCode, string message)
        => new() { ResponseCode = 1, StatusCode = statusCode, ResponseMsg = message };
}

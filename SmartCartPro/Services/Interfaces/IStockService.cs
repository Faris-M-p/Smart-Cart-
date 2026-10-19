using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IStockService
{
    // ── Existing members (other modules may call them) ────────────────────────────────
    Task<OutputStockMovement> ApplyOpeningStockAsync(InOpeningStockView view, int userId, CancellationToken ct = default);
    Task<OutputStockMovement> AdjustStockAsync(InStockAdjustView view, int userId, CancellationToken ct = default);
    Task<OutputStockTransfer> TransferStockAsync(InputStockTransfer input, CancellationToken ct = default);
    Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default);
    Task<OutputStockMovementList> GetMovementHistoryAsync(InputStockMovementList input, CancellationToken ct = default);
    Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default);

    // ── Input-based members used by the Inventory screens ─────────────────────────────

    /// <summary>First stock entry for a SKU at a store. Refused when opening stock was already recorded for it.</summary>
    Task<OutputStockMovement> ApplyOpeningStockAsync(InputStockOpening input, CancellationToken ct = default);

    /// <summary>Adds or removes stock for a SKU with a reason. Removing more than is on hand is refused.</summary>
    Task<OutputStockMovement> AdjustStockAsync(InputStockAdjust input, CancellationToken ct = default);

    /// <summary>Multi-line store-to-store transfer, applied atomically (every line moves or none does).</summary>
    Task<OutputStockTransferCreate> CreateStockTransferAsync(InputStockTransferCreate input, CancellationToken ct = default);

    /// <summary>Stock level of one SKU at one store (zero when it has no row yet); null when the SKU does not exist.</summary>
    Task<OutputInventorySelect?> GetStockLevelAsync(InputInventorySelect input, CancellationToken ct = default);

    /// <summary>Product / variant typeahead results with on-hand at the given store.</summary>
    Task<OutputStockProductSearch> SearchProductsAsync(InputStockProductSearch input, CancellationToken ct = default);

    /// <summary>Stores the given store may transfer stock to (same company, active, excluding itself).</summary>
    Task<List<OutputStockLookupItem>> GetTransferStoresAsync(int fromStoreId, CancellationToken ct = default);
}

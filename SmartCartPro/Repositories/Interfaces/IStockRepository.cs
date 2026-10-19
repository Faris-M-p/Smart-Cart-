using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

/// <summary>
/// Central gateway for all inventory changes.
/// Every stock mutation must go through <see cref="ApplyMovementAsync"/>
/// which writes a stock_movement record AND updates the inventory balance in a single transaction.
/// </summary>
public interface IStockRepository
{
    /// <summary>
    /// Applies a stock movement and updates the inventory balance atomically.
    /// Joins the caller's transaction when one is already open (sale, purchase and online-order flows do this);
    /// otherwise opens and commits its own.
    /// Negative-stock prevention is enforced for outbound movement types (sale, adjustment remove, transfer_out, return_out);
    /// a <c>sale</c> may go negative only when the setting <c>pos.allow_negative_stock</c> is true.
    /// </summary>
    Task<OutputStockMovement> ApplyMovementAsync(InputStockMovement input, CancellationToken ct = default);

    /// <summary>Single-line store-to-store transfer. Delegates to <see cref="CreateTransferAsync"/> (one atomic transaction).</summary>
    Task<OutputStockTransfer> TransferAsync(InputStockTransfer input, CancellationToken ct = default);

    Task<OutputInventorySelect?> GetInventoryAsync(InputInventorySelect input, CancellationToken ct = default);
    Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default);
    Task<OutputStockMovementList> ListMovementsAsync(InputStockMovementList input, CancellationToken ct = default);
    Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default);

    /// <summary>
    /// Stock level of one SKU at one store, including SKUs that have no inventory row yet (reported as zero on hand).
    /// Returns null when the product (or the variant of that product) does not exist.
    /// </summary>
    Task<OutputInventorySelect?> GetStockLevelAsync(InputInventorySelect input, CancellationToken ct = default);

    /// <summary>True when an <c>opening</c> movement was already recorded for the SKU at the store.</summary>
    Task<bool> HasOpeningStockAsync(InputInventorySelect input, CancellationToken ct = default);

    /// <summary>
    /// Moves every line out of the source store and into the destination store inside ONE database transaction.
    /// Any failing line rolls back the whole transfer.
    /// </summary>
    Task<OutputStockTransferCreate> CreateTransferAsync(InputStockTransferCreate input, CancellationToken ct = default);

    /// <summary>Product / variant typeahead used by the stock screens (name, code, SKU or barcode), with on-hand at the given store.</summary>
    Task<OutputStockProductSearch> SearchProductsAsync(InputStockProductSearch input, CancellationToken ct = default);

    /// <summary>Non-cancelled categories for the stock list category filter.</summary>
    Task<List<OutputStockLookupItem>> ListCategoryOptionsAsync(CancellationToken ct = default);

    /// <summary>Active stores of the same company as <paramref name="fromStoreId"/>, excluding that store (transfer destinations).</summary>
    Task<List<OutputStockLookupItem>> ListTransferStoresAsync(int fromStoreId, CancellationToken ct = default);
}

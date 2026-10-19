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
    /// Negative-stock prevention is enforced for outbound movement types (sale, adjustment remove, transfer_out).
    /// </summary>
    Task<OutputStockMovement> ApplyMovementAsync(InputStockMovement input, CancellationToken ct = default);

    Task<OutputStockTransfer> TransferAsync(InputStockTransfer input, CancellationToken ct = default);

    Task<OutputInventorySelect?> GetInventoryAsync(InputInventorySelect input, CancellationToken ct = default);
    Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default);
    Task<OutputStockMovementList> ListMovementsAsync(InputStockMovementList input, CancellationToken ct = default);
    Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default);
}

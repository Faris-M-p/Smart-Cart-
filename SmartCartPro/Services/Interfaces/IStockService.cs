using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IStockService
{
    Task<OutputStockMovement> ApplyOpeningStockAsync(InOpeningStockView view, int userId, CancellationToken ct = default);
    Task<OutputStockMovement> AdjustStockAsync(InStockAdjustView view, int userId, CancellationToken ct = default);
    Task<OutputStockTransfer> TransferStockAsync(InputStockTransfer input, CancellationToken ct = default);
    Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default);
    Task<OutputStockMovementList> GetMovementHistoryAsync(InputStockMovementList input, CancellationToken ct = default);
    Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default);
}

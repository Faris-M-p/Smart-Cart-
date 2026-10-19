using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _repo;

    public StockService(IStockRepository repo) => _repo = repo;

    public Task<OutputStockMovement> ApplyOpeningStockAsync(InOpeningStockView view, int userId, CancellationToken ct = default)
        => _repo.ApplyMovementAsync(new InputStockMovement
        {
            FkStore = view.FkStore,
            FkProduct = view.FkProduct,
            FkProductVariant = view.FkProductVariant,
            MovementType = "opening",
            Quantity = view.Quantity,
            Notes = view.Notes,
            CreatedBy = userId
        }, ct);

    public Task<OutputStockMovement> AdjustStockAsync(InStockAdjustView view, int userId, CancellationToken ct = default)
    {
        bool isAdd = string.Equals(view.Direction, "add", StringComparison.OrdinalIgnoreCase);
        // "adjustment" movement: positive quantity = stock increase, negative = decrease.
        decimal signedQty = isAdd ? Math.Abs(view.Quantity) : -Math.Abs(view.Quantity);

        return _repo.ApplyMovementAsync(new InputStockMovement
        {
            FkStore = view.FkStore,
            FkProduct = view.FkProduct,
            FkProductVariant = view.FkProductVariant,
            MovementType = "adjustment",
            Quantity = signedQty,
            Notes = view.Notes,
            CreatedBy = userId
        }, ct);
    }

    public Task<OutputStockTransfer> TransferStockAsync(InputStockTransfer input, CancellationToken ct = default)
        => _repo.TransferAsync(input, ct);

    public Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default)
        => _repo.ListInventoryAsync(input, ct);

    public Task<OutputStockMovementList> GetMovementHistoryAsync(InputStockMovementList input, CancellationToken ct = default)
        => _repo.ListMovementsAsync(input, ct);

    public Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default)
        => _repo.GetLowStockAsync(input, ct);
}

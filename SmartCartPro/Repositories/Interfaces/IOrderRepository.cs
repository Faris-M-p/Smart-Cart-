using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

/// <summary>
/// Online order repository — rewritten in Phase 7 to use Input/Output models.
/// Entities stay internal to this repository only.
/// </summary>
public interface IOnlineOrderRepository
{
    Task<OutputOnlineOrderSelect?> GetOrderAsync(InputOnlineOrderSelect input, CancellationToken ct = default);
    Task<OutputOnlineOrderList> ListOrdersAsync(InputOnlineOrderList input, CancellationToken ct = default);
    Task<OutputOnlineOrderStatusUpdate> UpdateStatusAsync(InputOnlineOrderStatusUpdate input, CancellationToken ct = default);
}

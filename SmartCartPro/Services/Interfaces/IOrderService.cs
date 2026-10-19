using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

/// <summary>
/// Online order service — Phase 7 rewrite. All entity references removed.
/// </summary>
public interface IOnlineOrderService
{
    Task<OutputOnlineOrderSelect?> GetOrderAsync(InputOnlineOrderSelect input, CancellationToken ct = default);
    Task<OutputOnlineOrderList> ListOrdersAsync(InputOnlineOrderList input, CancellationToken ct = default);
    Task<OutputOnlineOrderStatusUpdate> UpdateStatusAsync(InputOnlineOrderStatusUpdate input, CancellationToken ct = default);
}

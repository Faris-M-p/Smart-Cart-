using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Online order service — Phase 7 rewrite.
/// Delegates directly to IOnlineOrderRepository; thin orchestration layer.
/// </summary>
public class OnlineOrderService : IOnlineOrderService
{
    private readonly IOnlineOrderRepository _repo;

    public OnlineOrderService(IOnlineOrderRepository repo) => _repo = repo;

    public Task<OutputOnlineOrderSelect?> GetOrderAsync(
        InputOnlineOrderSelect input, CancellationToken ct = default) =>
        _repo.GetOrderAsync(input, ct);

    public Task<OutputOnlineOrderList> ListOrdersAsync(
        InputOnlineOrderList input, CancellationToken ct = default) =>
        _repo.ListOrdersAsync(input, ct);

    public Task<OutputOnlineOrderStatusUpdate> UpdateStatusAsync(
        InputOnlineOrderStatusUpdate input, CancellationToken ct = default) =>
        _repo.UpdateStatusAsync(input, ct);
}

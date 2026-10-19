using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _repo;

    public PurchaseService(IPurchaseRepository repo) => _repo = repo;

    public Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default) => _repo.GetPurchaseAsync(input, ct);
    public Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default) => _repo.ListPurchasesAsync(input, ct);
    public Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default) => _repo.CreatePurchaseAsync(input, ct);
    public Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default) => _repo.UpdatePurchaseAsync(input, ct);
    public Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default) => _repo.ReceiveGoodsAsync(input, ct);
    public Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default) => _repo.CancelPurchaseAsync(input, ct);
}

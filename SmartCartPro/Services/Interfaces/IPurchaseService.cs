using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IPurchaseService
{
    Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default);
    Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default);
    Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default);
    Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default);
    Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default);
    Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default);
}

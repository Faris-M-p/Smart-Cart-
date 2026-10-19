using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IPurchaseRepository
{
    Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default);
    Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default);
    Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default);
    Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default);

    /// <summary>
    /// Partial/full goods receipt: updates received quantities, calls stock ApplyMovement(purchase),
    /// sets PO status to partially_received or received.
    /// </summary>
    Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default);

    Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default);
}

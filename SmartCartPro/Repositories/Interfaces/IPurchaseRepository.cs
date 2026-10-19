using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IPurchaseRepository
{
    /// <summary>Header, lines (with received quantities) and receipt history of one purchase order in the given store.</summary>
    Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default);

    Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default);

    /// <summary>Inserts header and lines in a single save (atomic). Amounts arrive pre-computed from the service.</summary>
    Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default);

    /// <summary>Replaces header and lines of a draft / posted purchase order that has no receipts yet.</summary>
    Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default);

    /// <summary>
    /// Posts one stock movement per received line, then moves the purchase order to its new status.
    /// Must not run inside an outer transaction — each stock movement owns its transaction.
    /// </summary>
    Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default);

    Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default);

    /// <summary>Active products (plus their active variants) for the line picker, with cost price and tax rate.</summary>
    Task<IReadOnlyList<OutputPurchaseProductSelect>> ListProductsAsync(InputPurchaseProductList input, CancellationToken ct = default);
}

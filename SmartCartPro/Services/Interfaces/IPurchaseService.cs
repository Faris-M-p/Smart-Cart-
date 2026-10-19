using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IPurchaseService
{
    /// <summary>One purchase order of the given store, with received quantities and the allowed actions.</summary>
    Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default);

    Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default);

    /// <summary>Validates the document, recomputes tax and totals from master data, then saves it (draft or posted).</summary>
    Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default);

    /// <summary>Re-validates and replaces a draft / posted purchase order that has no receipts yet.</summary>
    Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default);

    /// <summary>Posts a goods receipt (GRN); quantities above what is still pending are rejected.</summary>
    Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default);

    Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default);

    /// <summary>Product picker used by the purchase form (name / code / SKU / barcode search).</summary>
    Task<IReadOnlyList<OutputPurchaseProductSelect>> ListProductsAsync(InputPurchaseProductList input, CancellationToken ct = default);
}

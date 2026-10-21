using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISaleService
{
    Task<OutputPosSelect> GetPosAsync(InputPosSelect input, CancellationToken ct = default);
    Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default);
    Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default);
    Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default);
    Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default);
    Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default);
    Task<OutputSaleReturnableSelect> GetReturnableSaleAsync(InputSaleReturnableSelect input, CancellationToken ct = default);
    Task<OutputSalesReturnList> ListSalesReturnsAsync(InputSalesReturnList input, CancellationToken ct = default);
    Task<OutputSalesReturnSelect?> GetSalesReturnAsync(InputSalesReturnSelect input, CancellationToken ct = default);

    Task<IReadOnlyList<OutputSaleProductSelect>> SearchProductsAsync(InputSaleProductSearch input, CancellationToken ct = default);
    Task<OutputSaleProductSelect> GetProductByBarcodeAsync(InputSaleProductBarcode input, CancellationToken ct = default);

    Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default);
    Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default);
    Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default);
    Task<OutputCashSessionList> ListSessionsAsync(InputCashSessionList input, CancellationToken ct = default);
    Task<OutputCashSummary> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default);
}

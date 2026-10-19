using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISaleService
{
    Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default);
    Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default);
    Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default);
    Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default);
    Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default);

    Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default);
    Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default);
    Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default);
    Task<OutputCashSummary> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default);
}

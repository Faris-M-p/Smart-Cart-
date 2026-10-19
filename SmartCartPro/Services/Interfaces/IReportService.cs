using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IReportService
{
    Task<OutputReportSalesByDate> GetSalesByDateAsync(InputReportSalesByDate input, CancellationToken ct = default);
    Task<OutputReportStockValuation> GetStockValuationAsync(InputReportStockValuation input, CancellationToken ct = default);
    Task<OutputLowStockList> GetLowStockReportAsync(InputReportLowStock input, CancellationToken ct = default);
    Task<OutputReportTaxSummary> GetTaxSummaryAsync(InputReportTaxSummary input, CancellationToken ct = default);
    Task<OutputReportTopSelling> GetTopSellingAsync(InputReportTopSelling input, CancellationToken ct = default);
}

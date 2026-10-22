using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IReportService
{
    Task<OutputReportIndex> GetIndexAsync(InputReportIndex input, CancellationToken ct = default);
    Task<OutputReportSales> GetSalesAsync(InputReportSales input, CancellationToken ct = default);
    Task<OutputReportProducts> GetProductsAsync(InputReportProducts input, CancellationToken ct = default);
    Task<OutputReportStockValuation> GetStockValuationAsync(InputReportStockValuation input, CancellationToken ct = default);
    Task<OutputReportTax> GetTaxAsync(InputReportTax input, CancellationToken ct = default);
    Task<OutputReportPurchases> GetPurchasesAsync(InputReportPurchases input, CancellationToken ct = default);
    Task<OutputReportCustomers> GetCustomersAsync(InputReportCustomers input, CancellationToken ct = default);
    Task<OutputReportScreen> GetScreenAsync(InputReportQuery input, CancellationToken ct = default);
}

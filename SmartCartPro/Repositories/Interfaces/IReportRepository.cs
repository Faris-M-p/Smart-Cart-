using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IReportRepository
{
    Task<List<OutputReportStoreOption>> ListActiveStoresAsync(CancellationToken ct = default);
    Task<List<OutputReportCategoryOption>> ListActiveCategoriesAsync(CancellationToken ct = default);
    Task<bool> ActiveStoreExistsAsync(int idStore, CancellationToken ct = default);

    Task<List<OutputReportSaleFact>> ListSaleFactsAsync(InputReportSales input, CancellationToken ct = default);
    Task<List<OutputReportReturnFact>> ListReturnFactsAsync(InputReportSales input, CancellationToken ct = default);
    Task<List<OutputReportPaymentSlice>> ListSalesPaymentsAsync(InputReportSales input, CancellationToken ct = default);

    Task<List<OutputReportProductRow>> ListTopProductsAsync(InputReportProducts input, CancellationToken ct = default);

    Task<List<OutputReportStockValuationRow>> ListStockValuationAsync(InputReportStockValuation input, CancellationToken ct = default);

    Task<List<OutputReportTaxRow>> ListTaxSummaryAsync(InputReportTax input, CancellationToken ct = default);

    Task<List<OutputReportPurchaseRow>> ListPurchasesBySupplierAsync(InputReportPurchases input, CancellationToken ct = default);

    Task<List<OutputReportCustomerRow>> ListTopCustomersAsync(InputReportCustomers input, CancellationToken ct = default);
}

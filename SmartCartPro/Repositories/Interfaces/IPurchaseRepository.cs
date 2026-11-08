using SmartCartPro.Entities;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IPurchaseRepository
{
    #region Purchases

    Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default);
    Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default);
    Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default);
    Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default);
    Task ApprovePurchaseAsync(int id, int storeId, int userId, CancellationToken ct = default);
    Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default);
    Task<IReadOnlyList<OutputPurchaseProductSelect>> ListProductsAsync(InputPurchaseProductList input, CancellationToken ct = default);

    #endregion

    #region Purchase Returns

    Task<OutputPurchaseReturnSelect?> GetReturnAsync(int id, int storeId, CancellationToken ct = default);
    Task<OutputPurchaseReturnList> ListReturnsAsync(InputPurchaseReturnList input, CancellationToken ct = default);
    Task<int> CreateReturnAsync(InPurchaseReturnCreateView input, int storeId, int userId, CancellationToken ct = default);
    Task ApproveReturnAsync(int id, int storeId, int userId, CancellationToken ct = default);
    Task CancelReturnAsync(int id, int storeId, string reason, CancellationToken ct = default);
    Task<IReadOnlyList<OutputPurchaseListItem>> ListReturnSourcePurchasesAsync(int storeId, string? search = null, CancellationToken ct = default);

    #endregion

    #region Purchase Orders

    Task<OutputPurchaseOrderSelect?> GetOrderAsync(int id, int storeId, CancellationToken ct = default);
    Task<OutputPurchaseOrderList> ListOrdersAsync(InputPurchaseOrderList input, CancellationToken ct = default);
    Task<int> SaveOrderAsync(InPurchaseOrderView input, int storeId, int userId, CancellationToken ct = default);
    Task DeleteOrderAsync(int id, int storeId, CancellationToken ct = default);

    #endregion

    #region Supplier Financials & Balance

    Task<OutputSupplierBalance> GetSupplierBalanceAsync(int supplierId, int storeId, CancellationToken ct = default);
    Task<OutputSupplierStatement> GetSupplierStatementAsync(int supplierId, int storeId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
    Task<OutputSupplierTransactionList> ListSupplierTransactionsAsync(InputSupplierTransactionList input, CancellationToken ct = default);
    Task<int> RecordSupplierTransactionAsync(InSupplierPaymentView input, int storeId, int userId, CancellationToken ct = default);
    Task ReverseSupplierTransactionAsync(int id, int storeId, int userId, string reason, CancellationToken ct = default);

    #endregion
}

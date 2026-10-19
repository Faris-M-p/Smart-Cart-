using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISupplierService
{
    Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default);
    Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default);
    Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default);
    Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default);
    Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default);
}

using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISupplierService
{
    Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default);
    Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default);
    Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default);
    Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default);
    Task<OutputSupplierSetActive> SetSupplierActiveAsync(InputSupplierSetActive input, CancellationToken ct = default);

    /// <summary>Active suppliers for dropdowns.</summary>
    Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default);

    /// <summary>Suppliers for dropdowns; pass <c>true</c> to include inactive ones (filters, existing documents).</summary>
    Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(bool includeInactive, CancellationToken ct = default);
}

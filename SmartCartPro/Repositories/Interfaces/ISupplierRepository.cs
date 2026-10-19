using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ISupplierRepository
{
    Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default);
    Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default);
    Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default);
    Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default);
    Task<OutputSupplierSetActive> SetSupplierActiveAsync(InputSupplierSetActive input, CancellationToken ct = default);

    /// <summary>Lookup list for dropdowns (active suppliers only).</summary>
    Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default);

    /// <summary>Lookup list for dropdowns; pass <c>true</c> to include inactive suppliers (list filters, existing documents).</summary>
    Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(bool includeInactive, CancellationToken ct = default);
}

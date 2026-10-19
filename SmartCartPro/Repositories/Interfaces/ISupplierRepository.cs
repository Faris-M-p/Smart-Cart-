using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ISupplierRepository
{
    Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default);
    Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default);
    Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default);
    Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default);

    /// <summary>Lookup list for dropdown (id + name, active only).</summary>
    Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default);
}

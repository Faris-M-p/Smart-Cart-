using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;

    public SupplierService(ISupplierRepository repo) => _repo = repo;

    public Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default) => _repo.GetSupplierAsync(input, ct);
    public Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default) => _repo.ListSuppliersAsync(input, ct);
    public Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default) => _repo.CreateSupplierAsync(input, ct);
    public Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default) => _repo.UpdateSupplierAsync(input, ct);
    public Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default) => _repo.GetSupplierLookupAsync(ct);
}

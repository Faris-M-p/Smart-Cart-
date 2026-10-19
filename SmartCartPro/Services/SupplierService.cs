using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class SupplierService : ISupplierService
{
    private static readonly int[] PageSizes = { 10, 25, 50, 100 };
    private static readonly string[] SortFields = { "suppliercode", "suppliername", "city", "isactive" };

    private readonly ISupplierRepository _repo;

    public SupplierService(ISupplierRepository repo) => _repo = repo;

    public Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default)
        => _repo.GetSupplierAsync(input, ct);

    public async Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default)
    {
        input.Search = Clean(input.Search);
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(SortFields, sort) >= 0 ? sort : "suppliername";

        var result = await _repo.ListSuppliersAsync(input, ct);

        // A stale link can point past the last page (e.g. after a filter change) — show the last page instead of an empty one.
        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListSuppliersAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default)
    {
        input.SupplierName = (input.SupplierName ?? string.Empty).Trim();
        RequireName(input.SupplierName);

        input.SupplierCode = Clean(input.SupplierCode);
        input.ContactPerson = Clean(input.ContactPerson);
        input.Email = Clean(input.Email);
        input.Phone = Clean(input.Phone);
        input.AddressLine1 = Clean(input.AddressLine1);
        input.City = Clean(input.City);
        input.State = Clean(input.State);
        input.PostalCode = Clean(input.PostalCode);
        input.Country = Clean(input.Country);
        input.TaxNumber = Clean(input.TaxNumber);

        var result = await _repo.CreateSupplierAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to create the supplier.");

        return result;
    }

    public async Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default)
    {
        input.SupplierName = (input.SupplierName ?? string.Empty).Trim();
        RequireName(input.SupplierName);

        input.SupplierCode = Clean(input.SupplierCode);
        input.ContactPerson = Clean(input.ContactPerson);
        input.Email = Clean(input.Email);
        input.Phone = Clean(input.Phone);
        input.AddressLine1 = Clean(input.AddressLine1);
        input.City = Clean(input.City);
        input.State = Clean(input.State);
        input.PostalCode = Clean(input.PostalCode);
        input.Country = Clean(input.Country);
        input.TaxNumber = Clean(input.TaxNumber);

        var result = await _repo.UpdateSupplierAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update the supplier.");

        return result;
    }

    public async Task<OutputSupplierSetActive> SetSupplierActiveAsync(InputSupplierSetActive input, CancellationToken ct = default)
    {
        var result = await _repo.SetSupplierActiveAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to change the supplier status.");

        return result;
    }

    public Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default)
        => _repo.GetSupplierLookupAsync(false, ct);

    public Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(bool includeInactive, CancellationToken ct = default)
        => _repo.GetSupplierLookupAsync(includeInactive, ct);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void RequireName(string name)
    {
        if (name.Length >= 2) return;

        const string message = "The Supplier Name must be between 2 and 300 characters.";
        throw new ValidationException(message, message, new Dictionary<string, string[]> { ["SupplierName"] = new[] { message } });
    }

    private static AppException Failure(int statusCode, string? message, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        var developer = $"Supplier repository returned {statusCode}: {text}";

        return statusCode switch
        {
            404 => new ResourceNotFoundException(text, developer),
            409 => new ConflictException(text, developer),
            422 => new ValidationException(text, developer),
            _ => new UpdateFailedException(text, developer)
        };
    }
}

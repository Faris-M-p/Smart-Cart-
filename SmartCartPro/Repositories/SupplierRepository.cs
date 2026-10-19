using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly SmartCartDbContext _db;

    public SupplierRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputSupplierSelect?> GetSupplierAsync(InputSupplierSelect input, CancellationToken ct = default)
        => await _db.Suppliers
            .Where(x => x.IdSupplier == input.IdSupplier && !x.Cancelled)
            .Select(SupplierProjection())
            .FirstOrDefaultAsync(ct);

    public async Task<OutputSupplierList> ListSuppliersAsync(InputSupplierList input, CancellationToken ct = default)
    {
        var q = _db.Suppliers.Where(x => !x.Cancelled);
        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x => x.SupplierName.Contains(input.Search)
                || (x.SupplierCode != null && x.SupplierCode.Contains(input.Search))
                || (x.Phone != null && x.Phone.Contains(input.Search)));

        q = input.SortBy.ToLowerInvariant() switch
        {
            _ => input.SortDesc ? q.OrderByDescending(x => x.SupplierName) : q.OrderBy(x => x.SupplierName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(SupplierProjection())
            .ToListAsync(ct);

        return new OutputSupplierList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputSupplierCreate> CreateSupplierAsync(InputSupplierCreate input, CancellationToken ct = default)
    {
        var entity = new Supplier
        {
            SupplierCode = input.SupplierCode,
            SupplierName = input.SupplierName,
            ContactPerson = input.ContactPerson,
            Email = input.Email,
            Phone = input.Phone,
            AddressLine1 = input.AddressLine1,
            City = input.City,
            State = input.State,
            PostalCode = input.PostalCode,
            Country = input.Country,
            TaxNumber = input.TaxNumber,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputSupplierCreate { IdSupplier = entity.IdSupplier, SupplierName = entity.SupplierName };
    }

    public async Task<OutputSupplierUpdate> UpdateSupplierAsync(InputSupplierUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Suppliers.FirstOrDefaultAsync(x => x.IdSupplier == input.IdSupplier && !x.Cancelled, ct);
        if (entity is null) return new OutputSupplierUpdate { ResponseCode = 1, ResponseMsg = "Supplier not found.", StatusCode = 404 };

        entity.SupplierCode = input.SupplierCode;
        entity.SupplierName = input.SupplierName;
        entity.ContactPerson = input.ContactPerson;
        entity.Email = input.Email;
        entity.Phone = input.Phone;
        entity.AddressLine1 = input.AddressLine1;
        entity.City = input.City;
        entity.State = input.State;
        entity.PostalCode = input.PostalCode;
        entity.Country = input.Country;
        entity.TaxNumber = input.TaxNumber;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return new OutputSupplierUpdate { IdSupplier = entity.IdSupplier };
    }

    public Task<IReadOnlyList<OutputSupplierSelect>> GetSupplierLookupAsync(CancellationToken ct = default)
        => _db.Suppliers.Where(x => !x.Cancelled && x.IsActive)
            .OrderBy(x => x.SupplierName)
            .Select(SupplierProjection())
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<OutputSupplierSelect>>(t => t.Result, ct);

    private static System.Linq.Expressions.Expression<Func<Supplier, OutputSupplierSelect>> SupplierProjection()
        => x => new OutputSupplierSelect
        {
            IdSupplier = x.IdSupplier,
            SupplierCode = x.SupplierCode,
            SupplierName = x.SupplierName,
            ContactPerson = x.ContactPerson,
            Email = x.Email,
            Phone = x.Phone,
            AddressLine1 = x.AddressLine1,
            City = x.City,
            State = x.State,
            PostalCode = x.PostalCode,
            Country = x.Country,
            TaxNumber = x.TaxNumber,
            IsActive = x.IsActive
        };
}

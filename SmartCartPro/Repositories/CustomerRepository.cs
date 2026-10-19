using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly SmartCartDbContext _db;

    public CustomerRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputCustomerSelect?> GetCustomerAsync(InputCustomerSelect input, CancellationToken ct = default)
        => await _db.Customers
            .Where(x => x.IdCustomer == input.IdCustomer && !x.Cancelled)
            .Select(CustomerProjection())
            .FirstOrDefaultAsync(ct);

    public async Task<OutputCustomerList> ListCustomersAsync(InputCustomerList input, CancellationToken ct = default)
    {
        var q = _db.Customers.Where(x => !x.Cancelled);
        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x =>
                x.FullName.Contains(input.Search) ||
                (x.Phone != null && x.Phone.Contains(input.Search)) ||
                (x.Email != null && x.Email.Contains(input.Search)));

        q = input.SortDesc ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(CustomerProjection())
            .ToListAsync(ct);

        return new OutputCustomerList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputCustomerCreate> CreateCustomerAsync(InputCustomerCreate input, CancellationToken ct = default)
    {
        var entity = new Customer
        {
            CustomerCode = input.CustomerCode,
            FullName = input.FullName,
            Email = input.Email,
            Phone = input.Phone,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new OutputCustomerCreate { IdCustomer = entity.IdCustomer, FullName = entity.FullName };
    }

    public async Task<OutputCustomerUpdate> UpdateCustomerAsync(InputCustomerUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Customers.FirstOrDefaultAsync(x => x.IdCustomer == input.IdCustomer && !x.Cancelled, ct);
        if (entity is null) return new OutputCustomerUpdate { ResponseCode = 1, ResponseMsg = "Customer not found.", StatusCode = 404 };

        entity.CustomerCode = input.CustomerCode;
        entity.FullName = input.FullName;
        entity.Email = input.Email;
        entity.Phone = input.Phone;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputCustomerUpdate { IdCustomer = entity.IdCustomer };
    }

    public async Task<OutputCustomerSelect?> FindByPhoneAsync(string phone, CancellationToken ct = default)
        => await _db.Customers
            .Where(x => !x.Cancelled && x.IsActive && x.Phone == phone)
            .Select(CustomerProjection())
            .FirstOrDefaultAsync(ct);

    private static System.Linq.Expressions.Expression<Func<Customer, OutputCustomerSelect>> CustomerProjection()
        => x => new OutputCustomerSelect
        {
            IdCustomer = x.IdCustomer,
            CustomerCode = x.CustomerCode,
            FullName = x.FullName,
            Email = x.Email,
            Phone = x.Phone,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        };
}

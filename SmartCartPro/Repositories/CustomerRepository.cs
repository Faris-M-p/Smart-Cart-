using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        else if (input.ActiveOnly == false) q = q.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            var digits = input.PhoneDigits ?? string.Empty;
            q = q.Where(x =>
                x.FullName.ToLower().Contains(term)
                || (x.CustomerCode != null && x.CustomerCode.ToLower().Contains(term))
                || (x.Email != null && x.Email.ToLower().Contains(term))
                || (x.Phone != null && x.Phone.ToLower().Contains(term))
                || (digits.Length > 0 && x.Phone != null
                    && x.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace(".", "").Contains(digits)));
        }

        q = (input.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "customercode" => input.SortDesc
                ? q.OrderByDescending(x => x.CustomerCode).ThenBy(x => x.FullName)
                : q.OrderBy(x => x.CustomerCode).ThenBy(x => x.FullName),
            "phone" => input.SortDesc
                ? q.OrderByDescending(x => x.Phone).ThenBy(x => x.FullName)
                : q.OrderBy(x => x.Phone).ThenBy(x => x.FullName),
            "email" => input.SortDesc
                ? q.OrderByDescending(x => x.Email).ThenBy(x => x.FullName)
                : q.OrderBy(x => x.Email).ThenBy(x => x.FullName),
            "isactive" => input.SortDesc
                ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName)
                : q.OrderBy(x => x.IsActive).ThenBy(x => x.FullName),
            "createdat" => input.SortDesc
                ? q.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.FullName)
                : q.OrderBy(x => x.CreatedAt).ThenBy(x => x.FullName),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.FullName)
                : q.OrderBy(x => x.FullName)
        };

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
        var conflict = await FindConflictAsync(input.CustomerCode, input.Email, input.Phone, excludeId: null, ct);
        if (conflict is not null)
            return conflict.ToCreate();

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

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex, out var message))
        {
            return new OutputCustomerCreate { ResponseCode = 1, ResponseMsg = message, StatusCode = 409 };
        }

        if (string.IsNullOrWhiteSpace(entity.CustomerCode))
        {
            entity.CustomerCode = NextCode(entity.IdCustomer);
            await _db.SaveChangesAsync(ct);
        }

        return new OutputCustomerCreate
        {
            IdCustomer = entity.IdCustomer,
            CustomerCode = entity.CustomerCode,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone
        };
    }

    public async Task<OutputCustomerUpdate> UpdateCustomerAsync(InputCustomerUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Customers.FirstOrDefaultAsync(x => x.IdCustomer == input.IdCustomer && !x.Cancelled, ct);
        if (entity is null)
            return new OutputCustomerUpdate { ResponseCode = 1, ResponseMsg = "Customer not found.", StatusCode = 404 };

        var conflict = await FindConflictAsync(input.CustomerCode, input.Email, input.Phone, input.IdCustomer, ct);
        if (conflict is not null)
            return new OutputCustomerUpdate { ResponseCode = conflict.ResponseCode, ResponseMsg = conflict.ResponseMsg, StatusCode = 409 };

        entity.CustomerCode = string.IsNullOrWhiteSpace(input.CustomerCode) ? entity.CustomerCode : input.CustomerCode;
        if (string.IsNullOrWhiteSpace(entity.CustomerCode))
            entity.CustomerCode = NextCode(entity.IdCustomer);
        entity.FullName = input.FullName;
        entity.Email = input.Email;
        entity.Phone = input.Phone;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex, out var message))
        {
            return new OutputCustomerUpdate { ResponseCode = 2, ResponseMsg = message, StatusCode = 409 };
        }

        return new OutputCustomerUpdate { IdCustomer = entity.IdCustomer };
    }

    public async Task<OutputCustomerSetActive> SetCustomerActiveAsync(InputCustomerSetActive input, CancellationToken ct = default)
    {
        var entity = await _db.Customers.FirstOrDefaultAsync(x => x.IdCustomer == input.IdCustomer && !x.Cancelled, ct);
        if (entity is null)
            return new OutputCustomerSetActive { ResponseCode = 1, ResponseMsg = "Customer not found.", StatusCode = 404 };

        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputCustomerSetActive { IdCustomer = entity.IdCustomer, IsActive = entity.IsActive };
    }

    public async Task<IReadOnlyList<OutputCustomerSearchItem>> SearchCustomersAsync(InputCustomerSearch input, CancellationToken ct = default)
    {
        var q = _db.Customers.Where(x => !x.Cancelled && x.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Term))
        {
            var term = input.Term.Trim().ToLower();
            var digits = input.PhoneDigits ?? string.Empty;
            q = q.Where(x =>
                x.FullName.ToLower().Contains(term)
                || (x.CustomerCode != null && x.CustomerCode.ToLower().Contains(term))
                || (x.Phone != null && x.Phone.ToLower().Contains(term))
                || (digits.Length > 0 && x.Phone != null
                    && x.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace(".", "").Contains(digits)));
        }

        return await q
            .OrderBy(x => x.FullName)
            .Take(input.Take)
            .Select(x => new OutputCustomerSearchItem
            {
                IdCustomer = x.IdCustomer,
                CustomerCode = x.CustomerCode,
                FullName = x.FullName,
                Phone = x.Phone,
                Email = x.Email
            })
            .ToListAsync(ct);
    }

    public async Task<OutputCustomerSelect?> FindByPhoneAsync(string phone, CancellationToken ct = default)
    {
        var digits = DigitsOnly(phone);
        return await _db.Customers
            .Where(x => !x.Cancelled && x.IsActive && x.Phone != null
                        && (x.Phone == phone
                            || (digits.Length > 0
                                && x.Phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace(".", "").Contains(digits))))
            .Select(CustomerProjection())
            .FirstOrDefaultAsync(ct);
    }

    private async Task<ConflictResult?> FindConflictAsync(
        string? code, string? email, string? phone, int? excludeId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeKey = code.Trim().ToLower();
            var exists = await _db.Customers.AnyAsync(
                x => x.CustomerCode != null
                     && x.CustomerCode.ToLower() == codeKey
                     && (!excludeId.HasValue || x.IdCustomer != excludeId.Value),
                ct);
            if (exists)
                return new ConflictResult("A customer with this code already exists.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailKey = email.Trim().ToLower();
            var exists = await _db.Customers.AnyAsync(
                x => x.Email != null
                     && x.Email.ToLower() == emailKey
                     && (!excludeId.HasValue || x.IdCustomer != excludeId.Value),
                ct);
            if (exists)
                return new ConflictResult("A customer with this email already exists.");
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var exists = await _db.Customers.AnyAsync(
                x => x.Phone != null
                     && x.Phone == phone
                     && (!excludeId.HasValue || x.IdCustomer != excludeId.Value),
                ct);
            if (exists)
                return new ConflictResult("A customer with this phone already exists.");
        }

        return null;
    }

    private static bool IsUniqueViolation(DbUpdateException ex, out string message)
    {
        message = "A customer with this email or phone already exists.";
        if (ex.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
            return false;

        var constraint = pg.ConstraintName ?? string.Empty;
        if (constraint.Contains("email", StringComparison.OrdinalIgnoreCase))
            message = "A customer with this email already exists.";
        else if (constraint.Contains("phone", StringComparison.OrdinalIgnoreCase))
            message = "A customer with this phone already exists.";
        return true;
    }

    private static string DigitsOnly(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    private static string NextCode(int id) => $"CUS-{id.ToString().PadLeft(6, '0')}";

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

    private sealed class ConflictResult
    {
        public ConflictResult(string message) => ResponseMsg = message;
        public int ResponseCode { get; } = 1;
        public string ResponseMsg { get; }

        public OutputCustomerCreate ToCreate() => new()
        {
            ResponseCode = ResponseCode,
            ResponseMsg = ResponseMsg,
            StatusCode = 409
        };
    }
}

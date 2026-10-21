using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly SmartCartDbContext _db;

    public CompanyRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputCompanySelect?> GetCompanyAsync(InputCompanySelect input, CancellationToken ct = default)
    {
        var row = await _db.Companies.AsNoTracking()
            .Where(c => !c.Cancelled)
            .Select(c => new OutputCompanySelect
            {
                IdCompany = c.IdCompany,
                CompanyName = c.CompanyName,
                CompanyCode = c.CompanyCode,
                LegalName = c.LegalName,
                TaxNumber = c.TaxNumber,
                RegistrationNo = c.RegistrationNo,
                Email = c.Email,
                Phone = c.Phone,
                Website = c.Website,
                AddressLine1 = c.AddressLine1,
                AddressLine2 = c.AddressLine2,
                City = c.City,
                State = c.State,
                PostalCode = c.PostalCode,
                Country = c.Country,
                CurrencyCode = c.CurrencyCode,
                TimeZone = c.TimeZone,
                Locale = c.Locale,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            return null;
        }

        row.CanEdit = true;
        row.RequiresBootstrap = false;
        return row;
    }

    public async Task<OutputCompanyCreate> CreateCompanyAsync(InputCompanyCreate input, CancellationToken ct = default)
    {
        var code = input.CompanyCode.Trim().ToUpperInvariant();
        var duplicate = await _db.Companies.AnyAsync(c => c.CompanyCode == code && !c.Cancelled, ct);
        if (duplicate)
        {
            return new OutputCompanyCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A company with this code already exists.",
                StatusCode = 409
            };
        }

        var entity = new Company
        {
            CompanyName = input.CompanyName.Trim(),
            CompanyCode = code,
            LegalName = input.LegalName?.Trim(),
            TaxNumber = input.TaxNumber?.Trim(),
            RegistrationNo = input.RegistrationNo?.Trim(),
            Email = input.Email?.Trim(),
            Phone = input.Phone?.Trim(),
            Website = input.Website?.Trim(),
            AddressLine1 = input.AddressLine1?.Trim(),
            AddressLine2 = input.AddressLine2?.Trim(),
            City = input.City?.Trim(),
            State = input.State?.Trim(),
            PostalCode = input.PostalCode?.Trim(),
            Country = input.Country?.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode)
                ? "INR"
                : input.CurrencyCode.Trim().ToUpperInvariant(),
            TimeZone = string.IsNullOrWhiteSpace(input.TimeZone) ? "Asia/Kolkata" : input.TimeZone.Trim(),
            Locale = string.IsNullOrWhiteSpace(input.Locale) ? "en-IN" : input.Locale.Trim(),
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Companies.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new OutputCompanyCreate
        {
            ResponseCode = 0,
            ResponseMsg = "Created",
            StatusCode = 200,
            IdCompany = entity.IdCompany,
            CompanyName = entity.CompanyName,
            CompanyCode = entity.CompanyCode,
            LegalName = entity.LegalName,
            TaxNumber = entity.TaxNumber,
            RegistrationNo = entity.RegistrationNo,
            Email = entity.Email,
            Phone = entity.Phone,
            Website = entity.Website,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            CurrencyCode = entity.CurrencyCode,
            TimeZone = entity.TimeZone,
            Locale = entity.Locale,
            IsActive = entity.IsActive
        };
    }

    public async Task<OutputCompanyUpdate> UpdateCompanyAsync(InputCompanyUpdate input, CancellationToken ct = default)
    {
        var existing = await _db.Companies.FirstOrDefaultAsync(c => !c.Cancelled, ct);
        if (existing is null)
        {
            return new OutputCompanyUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Company not found.",
                StatusCode = 404
            };
        }

        existing.CompanyName = input.CompanyName.Trim();
        existing.CompanyCode = input.CompanyCode.Trim().ToUpperInvariant();
        existing.LegalName = input.LegalName?.Trim();
        existing.TaxNumber = input.TaxNumber?.Trim();
        existing.RegistrationNo = input.RegistrationNo?.Trim();
        existing.Email = input.Email?.Trim();
        existing.Phone = input.Phone?.Trim();
        existing.Website = input.Website?.Trim();
        existing.AddressLine1 = input.AddressLine1?.Trim();
        existing.AddressLine2 = input.AddressLine2?.Trim();
        existing.City = input.City?.Trim();
        existing.State = input.State?.Trim();
        existing.PostalCode = input.PostalCode?.Trim();
        existing.Country = input.Country?.Trim();
        existing.CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode)
            ? existing.CurrencyCode
            : input.CurrencyCode.Trim().ToUpperInvariant();
        existing.TimeZone = string.IsNullOrWhiteSpace(input.TimeZone) ? existing.TimeZone : input.TimeZone.Trim();
        existing.Locale = string.IsNullOrWhiteSpace(input.Locale) ? existing.Locale : input.Locale.Trim();
        existing.IsActive = input.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new OutputCompanyUpdate
        {
            ResponseCode = 0,
            ResponseMsg = "Updated",
            StatusCode = 200,
            IdCompany = existing.IdCompany,
            CompanyName = existing.CompanyName,
            CompanyCode = existing.CompanyCode,
            LegalName = existing.LegalName,
            TaxNumber = existing.TaxNumber,
            RegistrationNo = existing.RegistrationNo,
            Email = existing.Email,
            Phone = existing.Phone,
            Website = existing.Website,
            AddressLine1 = existing.AddressLine1,
            AddressLine2 = existing.AddressLine2,
            City = existing.City,
            State = existing.State,
            PostalCode = existing.PostalCode,
            Country = existing.Country,
            CurrencyCode = existing.CurrencyCode,
            TimeZone = existing.TimeZone,
            Locale = existing.Locale,
            IsActive = existing.IsActive
        };
    }

    public Task<OutputStoreSelect?> GetStoreAsync(InputStoreSelect input, CancellationToken ct = default) =>
        _db.Stores.AsNoTracking()
            .Where(s => s.IdStore == input.IdStore && !s.Cancelled)
            .Select(s => new OutputStoreSelect
            {
                IdStore = s.IdStore,
                StoreCode = s.StoreCode,
                StoreName = s.StoreName,
                Email = s.Email,
                Phone = s.Phone,
                AddressLine1 = s.AddressLine1,
                AddressLine2 = s.AddressLine2,
                City = s.City,
                State = s.State,
                PostalCode = s.PostalCode,
                Country = s.Country,
                IsDefault = s.IsDefault,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputStoreList> ListStoresAsync(InputStoreList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 100);
        var sortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "storename" : input.SortBy;

        var q = _db.Stores.AsNoTracking().Where(s => !s.Cancelled);
        if (input.ActiveOnly == true) q = q.Where(s => s.IsActive);
        else if (input.ActiveOnly == false) q = q.Where(s => !s.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim();
            q = q.Where(s => s.StoreName.Contains(term)
                || s.StoreCode.Contains(term)
                || (s.City != null && s.City.Contains(term)));
        }

        q = sortBy.ToLowerInvariant() switch
        {
            "storecode" or "code" => input.SortDesc
                ? q.OrderByDescending(s => s.StoreCode).ThenBy(s => s.StoreName)
                : q.OrderBy(s => s.StoreCode).ThenBy(s => s.StoreName),
            "city" => input.SortDesc
                ? q.OrderByDescending(s => s.City).ThenBy(s => s.StoreName)
                : q.OrderBy(s => s.City).ThenBy(s => s.StoreName),
            "isactive" or "status" => input.SortDesc
                ? q.OrderByDescending(s => s.IsActive).ThenBy(s => s.StoreName)
                : q.OrderBy(s => s.IsActive).ThenBy(s => s.StoreName),
            "isdefault" or "default" => input.SortDesc
                ? q.OrderByDescending(s => s.IsDefault).ThenBy(s => s.StoreName)
                : q.OrderBy(s => s.IsDefault).ThenBy(s => s.StoreName),
            _ => input.SortDesc
                ? q.OrderByDescending(s => s.StoreName)
                : q.OrderByDescending(s => s.IsDefault).ThenBy(s => s.StoreName)
        };

        var total = await q.CountAsync(ct);
        var stores = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new OutputStoreSelect
            {
                IdStore = s.IdStore,
                StoreCode = s.StoreCode,
                StoreName = s.StoreName,
                Email = s.Email,
                Phone = s.Phone,
                AddressLine1 = s.AddressLine1,
                AddressLine2 = s.AddressLine2,
                City = s.City,
                State = s.State,
                PostalCode = s.PostalCode,
                Country = s.Country,
                IsDefault = s.IsDefault,
                IsActive = s.IsActive
            })
            .ToListAsync(ct);

        var max = await _db.Licenses.AsNoTracking()
            .Where(l => !l.Cancelled)
            .Select(l => (int?)l.MaxStores)
            .FirstOrDefaultAsync(ct) ?? 1;

        var count = await GetActiveStoreCountAsync(ct);

        return new OutputStoreList
        {
            CurrentStoreCount = count,
            MaxStores = max,
            CanCreateStore = input.CanCreate && count < max,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Stores = stores,
            Filter = new InputStoreList
            {
                Search = input.Search,
                ActiveOnly = input.ActiveOnly,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDesc = input.SortDesc
            }
        };
    }

    public async Task<OutputStoreAccessibleList> ListAccessibleStoresAsync(
        InputStoreAccessibleList input,
        CancellationToken ct = default)
    {
        List<OutputStoreSelect> stores;
        if (input.AllActiveStores)
        {
            stores = await _db.Stores.AsNoTracking()
                .Where(s => !s.Cancelled && s.IsActive)
                .OrderByDescending(s => s.IsDefault)
                .ThenBy(s => s.StoreName)
                .Select(s => new OutputStoreSelect
                {
                    IdStore = s.IdStore,
                    StoreCode = s.StoreCode,
                    StoreName = s.StoreName,
                    Email = s.Email,
                    Phone = s.Phone,
                    AddressLine1 = s.AddressLine1,
                    AddressLine2 = s.AddressLine2,
                    City = s.City,
                    State = s.State,
                    PostalCode = s.PostalCode,
                    Country = s.Country,
                    IsDefault = s.IsDefault,
                    IsActive = s.IsActive
                })
                .ToListAsync(ct);
        }
        else
        {
            stores = await (
                from a in _db.UserStoreAccess.AsNoTracking()
                join s in _db.Stores.AsNoTracking() on a.FkStore equals s.IdStore
                where a.FkUser == input.IdUser
                      && !a.Cancelled
                      && !s.Cancelled
                      && s.IsActive
                orderby s.IsDefault descending, s.StoreName
                select new OutputStoreSelect
                {
                    IdStore = s.IdStore,
                    StoreCode = s.StoreCode,
                    StoreName = s.StoreName,
                    Email = s.Email,
                    Phone = s.Phone,
                    AddressLine1 = s.AddressLine1,
                    AddressLine2 = s.AddressLine2,
                    City = s.City,
                    State = s.State,
                    PostalCode = s.PostalCode,
                    Country = s.Country,
                    IsDefault = s.IsDefault,
                    IsActive = s.IsActive
                }).ToListAsync(ct);
        }

        return new OutputStoreAccessibleList { Stores = stores };
    }

    public Task<int> GetActiveStoreCountAsync(CancellationToken ct = default) =>
        _db.Stores.CountAsync(s => s.IsActive && !s.Cancelled, ct);

    public Task<int> GetActiveUserCountAsync(CancellationToken ct = default) =>
        _db.Users.CountAsync(u => u.IsActive && !u.Cancelled && !u.IsPlatformSuperAdmin, ct);

    public async Task<OutputStoreCreate> CreateStoreAsync(InputStoreCreate input, CancellationToken ct = default)
    {
        var code = input.StoreCode.Trim().ToUpperInvariant();
        var duplicate = await _db.Stores.AnyAsync(
            s => s.FkCompany == input.FkCompany && s.StoreCode == code && !s.Cancelled, ct);
        if (duplicate)
        {
            return new OutputStoreCreate
            {
                ResponseCode = 1,
                ResponseMsg = "A store with this code already exists.",
                StatusCode = 409
            };
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        if (input.IsDefault)
        {
            var currentDefaults = await _db.Stores
                .Where(s => s.FkCompany == input.FkCompany && s.IsDefault && !s.Cancelled)
                .ToListAsync(ct);
            foreach (var row in currentDefaults)
            {
                row.IsDefault = false;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
        }

        var entity = new Store
        {
            FkCompany = input.FkCompany,
            StoreCode = code,
            StoreName = input.StoreName.Trim(),
            Email = input.Email?.Trim(),
            Phone = input.Phone?.Trim(),
            AddressLine1 = input.AddressLine1?.Trim(),
            AddressLine2 = input.AddressLine2?.Trim(),
            City = input.City?.Trim(),
            State = input.State?.Trim(),
            PostalCode = input.PostalCode?.Trim(),
            Country = input.Country?.Trim(),
            IsDefault = input.IsDefault,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Stores.Add(entity);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new OutputStoreCreate
        {
            ResponseCode = 0,
            ResponseMsg = "Created",
            StatusCode = 200,
            IdStore = entity.IdStore,
            StoreCode = entity.StoreCode,
            StoreName = entity.StoreName,
            Email = entity.Email,
            Phone = entity.Phone,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive
        };
    }

    public async Task<OutputStoreUpdate> UpdateStoreAsync(InputStoreUpdate input, CancellationToken ct = default)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(
            s => s.IdStore == input.IdStore && !s.Cancelled, ct);

        if (store is null)
        {
            return new OutputStoreUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Store not found.",
                StatusCode = 404
            };
        }

        var code = input.StoreCode.Trim().ToUpperInvariant();
        var duplicate = await _db.Stores.AnyAsync(
            s => s.FkCompany == store.FkCompany
                 && s.StoreCode == code
                 && s.IdStore != store.IdStore
                 && !s.Cancelled, ct);
        if (duplicate)
        {
            return new OutputStoreUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "A store with this code already exists.",
                StatusCode = 409
            };
        }

        if (!input.IsActive && store.IsDefault)
        {
            return new OutputStoreUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Promote another store to default before deactivating the current default.",
                StatusCode = 409
            };
        }

        if (!input.IsDefault && store.IsDefault)
        {
            return new OutputStoreUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Promote another store to default before removing the default flag from this store.",
                StatusCode = 409
            };
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        if (input.IsDefault && !store.IsDefault)
        {
            var others = await _db.Stores
                .Where(s => s.FkCompany == store.FkCompany && s.IdStore != store.IdStore && s.IsDefault && !s.Cancelled)
                .ToListAsync(ct);
            foreach (var row in others)
            {
                row.IsDefault = false;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
            store.IsDefault = true;
        }

        store.StoreCode = code;
        store.StoreName = input.StoreName.Trim();
        store.Email = input.Email?.Trim();
        store.Phone = input.Phone?.Trim();
        store.AddressLine1 = input.AddressLine1?.Trim();
        store.AddressLine2 = input.AddressLine2?.Trim();
        store.City = input.City?.Trim();
        store.State = input.State?.Trim();
        store.PostalCode = input.PostalCode?.Trim();
        store.Country = input.Country?.Trim();
        store.IsActive = input.IsActive;
        store.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new OutputStoreUpdate
        {
            ResponseCode = 0,
            ResponseMsg = "Updated",
            StatusCode = 200,
            IdStore = store.IdStore,
            StoreCode = store.StoreCode,
            StoreName = store.StoreName,
            Email = store.Email,
            Phone = store.Phone,
            AddressLine1 = store.AddressLine1,
            AddressLine2 = store.AddressLine2,
            City = store.City,
            State = store.State,
            PostalCode = store.PostalCode,
            Country = store.Country,
            IsDefault = store.IsDefault,
            IsActive = store.IsActive
        };
    }

    public async Task<OutputStoreStatusUpdate> UpdateStoreStatusAsync(
        InputStoreStatusUpdate input,
        CancellationToken ct = default)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(
            s => s.IdStore == input.IdStore && !s.Cancelled, ct);

        if (store is null)
        {
            return new OutputStoreStatusUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Store not found.",
                StatusCode = 404
            };
        }

        if (!input.IsActive && store.IsDefault)
        {
            return new OutputStoreStatusUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Promote another store to default before deactivating the current default.",
                StatusCode = 409
            };
        }

        store.IsActive = input.IsActive;
        store.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputStoreStatusUpdate
        {
            ResponseCode = 0,
            ResponseMsg = "Updated",
            StatusCode = 200,
            IdStore = store.IdStore,
            IsActive = store.IsActive
        };
    }

    public async Task<OutputLicenseSelect?> GetLicenseAsync(InputLicenseSelect input, CancellationToken ct = default)
    {
        try
        {
            var row = await _db.Licenses.AsNoTracking()
                .Where(l => !l.Cancelled)
                .Select(l => new
                {
                    l.IdLicense,
                    l.LicenseKey,
                    l.LicenseType,
                    l.Status,
                    l.StartDate,
                    l.ExpiryDate,
                    l.MaxStores,
                    l.Notes
                })
                .FirstOrDefaultAsync(ct);

            if (row is null)
                return null;

            return new OutputLicenseSelect
            {
                IdLicense = row.IdLicense,
                LicenseKey = row.LicenseKey,
                LicenseType = row.LicenseType,
                Status = MapLicenseStatusFromDb(row.Status),
                StartDate = row.StartDate,
                ExpiryDate = row.ExpiryDate,
                MaxStores = row.MaxStores,
                Notes = row.Notes
            };
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return null;
        }
    }

    public async Task<OutputLicenseUpdate> UpdateLicenseAsync(InputLicenseUpdate input, CancellationToken ct = default)
    {
        var statusDb = MapLicenseStatusToDb(input.Status);

        var existing = await _db.Licenses.FirstOrDefaultAsync(l => !l.Cancelled, ct);
        if (existing is null)
        {
            var created = new License
            {
                LicenseKey = input.LicenseKey.Trim(),
                LicenseType = input.LicenseType.Trim(),
                Status = statusDb,
                StartDate = input.StartDate,
                ExpiryDate = input.ExpiryDate,
                MaxStores = input.MaxStores,
                Notes = input.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _db.Licenses.Add(created);
            await _db.SaveChangesAsync(ct);

            return new OutputLicenseUpdate
            {
                ResponseCode = 0,
                ResponseMsg = "Created",
                StatusCode = 200,
                IdLicense = created.IdLicense,
                LicenseKey = created.LicenseKey,
                LicenseType = created.LicenseType,
                Status = input.Status,
                StartDate = created.StartDate,
                ExpiryDate = created.ExpiryDate,
                MaxStores = created.MaxStores,
                Notes = created.Notes
            };
        }

        existing.LicenseKey = input.LicenseKey.Trim();
        existing.LicenseType = input.LicenseType.Trim();
        existing.Status = statusDb;
        existing.StartDate = input.StartDate;
        existing.ExpiryDate = input.ExpiryDate;
        existing.MaxStores = input.MaxStores;
        existing.Notes = input.Notes?.Trim();
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputLicenseUpdate
        {
            ResponseCode = 0,
            ResponseMsg = "Updated",
            StatusCode = 200,
            IdLicense = existing.IdLicense,
            LicenseKey = existing.LicenseKey,
            LicenseType = existing.LicenseType,
            Status = input.Status,
            StartDate = existing.StartDate,
            ExpiryDate = existing.ExpiryDate,
            MaxStores = existing.MaxStores,
            Notes = existing.Notes
        };
    }

    public async Task AddAuditLogAsync(InputAuditLogCreate input, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            FkUser = input.ActorUserId,
            EntityName = input.EntityName,
            EntityId = input.EntityId,
            Action = input.Action,
            Details = input.Details,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    private static int MapLicenseStatusFromDb(string? dbValue) =>
        dbValue?.Trim().ToLowerInvariant() switch
        {
            "active" => (int)CommonModel.LicenseStatus.Active,
            "suspended" => (int)CommonModel.LicenseStatus.Suspended,
            "expired" => (int)CommonModel.LicenseStatus.Expired,
            "trial" => (int)CommonModel.LicenseStatus.Trial,
            _ => throw new ArgumentOutOfRangeException(nameof(dbValue), dbValue, "Unknown license status in database.")
        };

    private static string MapLicenseStatusToDb(int status) =>
        (CommonModel.LicenseStatus)status switch
        {
            CommonModel.LicenseStatus.Active => "active",
            CommonModel.LicenseStatus.Suspended => "suspended",
            CommonModel.LicenseStatus.Expired => "expired",
            CommonModel.LicenseStatus.Trial => "trial",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown license status.")
        };
}

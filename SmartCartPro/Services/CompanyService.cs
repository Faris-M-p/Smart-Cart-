using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CompanyService : ICompanyService
{
    private static readonly string[] ValidLicenseStatus = { "active", "suspended", "expired", "trial" };

    private readonly ICompanyRepository _repo;

    public CompanyService(ICompanyRepository repo) => _repo = repo;

    public async Task<OutputCompanySelect> GetCompanyAsync(InputCompanySelect input, CancellationToken ct = default)
    {
        var co = await _repo.GetCompanyAsync(input, ct);
        if (co is null)
        {
            return new OutputCompanySelect
            {
                RequiresBootstrap = true,
                CanEdit = input.CanCreate
            };
        }

        co.CanEdit = true;
        co.RequiresBootstrap = false;
        return co;
    }

    public async Task<OutputCompanyCreate> CreateCompanyAsync(InputCompanyCreate input, CancellationToken ct = default)
    {
        if (!input.CanCreate)
        {
            return FailCreate("Only Super Admin can create the company.", 403);
        }

        var existing = await _repo.GetCompanyAsync(new InputCompanySelect(), ct);
        if (existing is not null)
        {
            return FailCreate("Company already exists.", 409);
        }

        var created = await _repo.CreateCompanyAsync(input, ct);
        await Audit(input.ActorUserId, "Company", created.IdCompany, "Create",
            $"Company created: {created.CompanyName}", ct);
        return created;
    }

    public async Task<OutputCompanyUpdate> UpdateCompanyAsync(InputCompanyUpdate input, CancellationToken ct = default)
    {
        var existing = await _repo.GetCompanyAsync(new InputCompanySelect(), ct);
        if (existing is null)
        {
            return new OutputCompanyUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Company not found.",
                StatusCode = 404
            };
        }

        var updated = await _repo.UpdateCompanyAsync(input, ct);
        if (updated.Ok)
        {
            await Audit(input.ActorUserId, "Company", updated.IdCompany, "Update",
                $"Company profile updated: {updated.CompanyName}", ct);
        }

        return updated;
    }

    public Task<OutputStoreList> ListStoresAsync(InputStoreList input, CancellationToken ct = default) =>
        _repo.ListStoresAsync(input, ct);

    public Task<OutputStoreSelect?> GetStoreAsync(InputStoreSelect input, CancellationToken ct = default) =>
        _repo.GetStoreAsync(input, ct);

    public async Task<OutputStoreCreate> CreateStoreAsync(InputStoreCreate input, CancellationToken ct = default)
    {
        if (!input.CanCreate)
        {
            return FailStoreCreate("Only Super Admin can create a store. Admins may edit existing stores.", 403);
        }

        var company = await _repo.GetCompanyAsync(new InputCompanySelect(), ct);
        if (company is null)
        {
            return FailStoreCreate("Company must exist before creating a store.", 400);
        }

        var lic = await _repo.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (lic is null || lic.Status is not ("active" or "trial"))
        {
            return FailStoreCreate("An active license is required to create stores.", 400);
        }

        var count = await _repo.GetActiveStoreCountAsync(ct);
        if (count >= lic.MaxStores)
        {
            return FailStoreCreate($"Store limit reached ({lic.MaxStores}).", 400);
        }

        input.FkCompany = company.IdCompany;
        input.IsDefault = count == 0 || input.IsDefault;

        var created = await _repo.CreateStoreAsync(input, ct);
        await Audit(input.ActorUserId, "Store", created.IdStore, "Create",
            $"Store created: {created.StoreCode} / {created.StoreName}", ct);
        return created;
    }

    public async Task<OutputStoreUpdate> UpdateStoreAsync(InputStoreUpdate input, CancellationToken ct = default)
    {
        var updated = await _repo.UpdateStoreAsync(input, ct);
        if (updated.Ok)
        {
            await Audit(input.ActorUserId, "Store", updated.IdStore, "Update",
                $"Store updated: {updated.StoreCode}", ct);
        }

        return updated;
    }

    public Task<OutputLicenseSelect?> GetLicenseAsync(InputLicenseSelect input, CancellationToken ct = default) =>
        _repo.GetLicenseAsync(input, ct);

    public async Task<OutputLicenseUpdate> UpdateLicenseAsync(InputLicenseUpdate input, CancellationToken ct = default)
    {
        if (!input.CanEditLicense)
        {
            return new OutputLicenseUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Only Super Admin can change license settings.",
                StatusCode = 403
            };
        }

        if (input.ExpiryDate < input.StartDate)
        {
            return new OutputLicenseUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Expiry must be on or after start date.",
                StatusCode = 400
            };
        }

        if (!ValidLicenseStatus.Contains(input.Status))
        {
            return new OutputLicenseUpdate
            {
                ResponseCode = 1,
                ResponseMsg = "Invalid license status.",
                StatusCode = 400
            };
        }

        var updated = await _repo.UpdateLicenseAsync(input, ct);
        if (updated.Ok)
        {
            await Audit(input.ActorUserId, "License", updated.IdLicense, "Update",
                $"License updated. Key={input.LicenseKey}, MaxStores={input.MaxStores}.", ct);
        }

        return updated;
    }

    private Task Audit(int actorUserId, string entity, int? entityId, string action, string details, CancellationToken ct) =>
        _repo.AddAuditLogAsync(new InputAuditLogCreate
        {
            ActorUserId = actorUserId,
            EntityName = entity,
            EntityId = entityId,
            Action = action,
            Details = details
        }, ct);

    private static OutputCompanyCreate FailCreate(string msg, int status) => new()
    {
        ResponseCode = 1,
        ResponseMsg = msg,
        StatusCode = status
    };

    private static OutputStoreCreate FailStoreCreate(string msg, int status) => new()
    {
        ResponseCode = 1,
        ResponseMsg = msg,
        StatusCode = status
    };
}

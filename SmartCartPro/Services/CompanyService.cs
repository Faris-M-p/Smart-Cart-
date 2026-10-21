using Microsoft.Extensions.Caching.Memory;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CompanyService : ICompanyService
{
    internal const string LicenseStatusCacheKey = "smartcart:license:status";

    private readonly ICompanyRepository _repo;
    private readonly IMemoryCache _cache;

    public CompanyService(ICompanyRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

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
            throw new ForbiddenException(
                "Only a platform administrator can create the company.",
                $"Actor {input.ActorUserId} is not allowed to create a company.");
        }

        var existing = await _repo.GetCompanyAsync(new InputCompanySelect(), ct);
        if (existing is not null)
        {
            throw new ConflictException(
                "Company already exists.",
                $"IdCompany={existing.IdCompany}");
        }

        var created = await _repo.CreateCompanyAsync(input, ct);
        if (!created.Ok)
        {
            ThrowStoreOrCompanyFailure(created.StatusCode, created.ResponseMsg, "Unable to create company.");
        }

        await Audit(input.ActorUserId, "Company", created.IdCompany, "Create",
            $"Company created: {created.CompanyName}", ct);
        return created;
    }

    public async Task<OutputCompanyUpdate> UpdateCompanyAsync(InputCompanyUpdate input, CancellationToken ct = default)
    {
        var existing = await _repo.GetCompanyAsync(new InputCompanySelect(), ct);
        if (existing is null)
        {
            throw new ResourceNotFoundException(
                "Company not found.",
                $"Actor {input.ActorUserId} tried to update a missing company.");
        }

        var updated = await _repo.UpdateCompanyAsync(input, ct);
        if (!updated.Ok)
        {
            ThrowStoreOrCompanyFailure(updated.StatusCode, updated.ResponseMsg, "Unable to save company.");
        }

        await Audit(input.ActorUserId, "Company", updated.IdCompany, "Update",
            $"Company profile updated: {updated.CompanyName}", ct);
        return updated;
    }

    public Task<OutputStoreList> ListStoresAsync(InputStoreList input, CancellationToken ct = default) =>
        _repo.ListStoresAsync(input, ct);

    public Task<OutputStoreSelect?> GetStoreAsync(InputStoreSelect input, CancellationToken ct = default) =>
        _repo.GetStoreAsync(input, ct);

    public Task<OutputStoreAccessibleList> ListAccessibleStoresAsync(
        InputStoreAccessibleList input,
        CancellationToken ct = default) =>
        _repo.ListAccessibleStoresAsync(input, ct);

    public async Task<OutputStoreCreate> CreateStoreAsync(InputStoreCreate input, CancellationToken ct = default)
    {
        if (!input.CanCreate)
        {
            throw new ForbiddenException(
                "Only a platform administrator can create a store.",
                $"Actor {input.ActorUserId} is not allowed to create a store.");
        }

        var company = await _repo.GetCompanyAsync(new InputCompanySelect(), ct);
        if (company is null)
        {
            throw new ValidationException(
                "Create the company before adding a store.",
                "Company row is missing.");
        }

        var count = await _repo.GetActiveStoreCountAsync(ct);
        var lic = await GetLicenseAsync(new InputLicenseSelect(), ct);
        if (lic is not null && count >= lic.MaxStores)
        {
            throw new ConflictException(
                $"Store limit reached ({lic.MaxStores}).",
                $"ActiveStores={count}, MaxStores={lic.MaxStores}");
        }

        input.FkCompany = company.IdCompany;
        if (count == 0)
        {
            input.IsDefault = true;
            input.IsActive = true;
        }
        else if (input.IsDefault)
        {
            input.IsActive = true;
        }

        var created = await _repo.CreateStoreAsync(input, ct);
        if (!created.Ok)
        {
            ThrowStoreOrCompanyFailure(created.StatusCode, created.ResponseMsg, "Unable to create store.");
        }

        await Audit(input.ActorUserId, "Store", created.IdStore, "Create",
            $"Store created: {created.StoreCode} / {created.StoreName}", ct);
        return created;
    }

    public async Task<OutputStoreUpdate> UpdateStoreAsync(InputStoreUpdate input, CancellationToken ct = default)
    {
        _ = await _repo.GetStoreAsync(new InputStoreSelect { IdStore = input.IdStore }, ct)
            ?? throw new ResourceNotFoundException("Store not found.", $"IdStore={input.IdStore}");

        if (input.IsDefault)
        {
            input.IsActive = true;
        }

        var updated = await _repo.UpdateStoreAsync(input, ct);
        if (!updated.Ok)
        {
            ThrowStoreOrCompanyFailure(updated.StatusCode, updated.ResponseMsg, "Unable to update store.");
        }

        await Audit(input.ActorUserId, "Store", updated.IdStore, "Update",
            $"Store updated: {updated.StoreCode}", ct);
        return updated;
    }

    public async Task<OutputStoreStatusUpdate> SetStoreActiveAsync(
        InputStoreStatusUpdate input,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetStoreAsync(new InputStoreSelect { IdStore = input.IdStore }, ct)
            ?? throw new ResourceNotFoundException("Store not found.", $"IdStore={input.IdStore}");

        if (input.IsActive == existing.IsActive)
        {
            return new OutputStoreStatusUpdate
            {
                ResponseCode = 0,
                StatusCode = 200,
                IdStore = existing.IdStore,
                IsActive = existing.IsActive
            };
        }

        if (input.IsActive)
        {
            var lic = await GetLicenseAsync(new InputLicenseSelect(), ct);
            var count = await _repo.GetActiveStoreCountAsync(ct);
            if (lic is not null && count >= lic.MaxStores)
            {
                throw new ConflictException(
                    $"Store limit reached ({lic.MaxStores}). Deactivate another store or raise the license limit.",
                    $"ActiveStores={count}, MaxStores={lic.MaxStores}");
            }
        }

        var updated = await _repo.UpdateStoreStatusAsync(input, ct);
        if (!updated.Ok)
        {
            ThrowStoreOrCompanyFailure(updated.StatusCode, updated.ResponseMsg, "Unable to update store status.");
        }

        await Audit(input.ActorUserId, "Store", updated.IdStore, input.IsActive ? "Activate" : "Deactivate",
            $"Store '{existing.StoreCode}' {(input.IsActive ? "activated" : "deactivated")}.", ct);
        return updated;
    }

    public async Task<OutputLicenseSelect?> GetLicenseAsync(InputLicenseSelect input, CancellationToken ct = default)
    {
        var lic = await _repo.GetLicenseAsync(input, ct);
        if (lic is null)
        {
            return null;
        }

        lic.ActiveStoreCount = await _repo.GetActiveStoreCountAsync(ct);
        lic.ActiveUserCount = await _repo.GetActiveUserCountAsync(ct);
        return lic;
    }

    public async Task<OutputLicenseUpdate> UpdateLicenseAsync(InputLicenseUpdate input, CancellationToken ct = default)
    {
        if (!input.CanEditLicense)
        {
            throw new ForbiddenException(
                "Only a platform administrator can change license settings.",
                $"Actor {input.ActorUserId} cannot edit the license.");
        }

        if (input.ExpiryDate < input.StartDate)
        {
            throw new ValidationException(
                "Please correct the highlighted fields.",
                "Expiry must be on or after start date.",
                new Dictionary<string, string[]>
                {
                    ["ExpiryDate"] = ["The Expiry Date must be greater than or equal to the Start Date."]
                });
        }

        if (!Enum.IsDefined(typeof(CommonModel.LicenseStatus), input.Status))
        {
            throw new ValidationException(
                "Please correct the highlighted fields.",
                $"Invalid license status {input.Status}.",
                new Dictionary<string, string[]>
                {
                    ["Status"] = ["Please select a valid status."]
                });
        }

        var activeStores = await _repo.GetActiveStoreCountAsync(ct);
        if (input.MaxStores < activeStores)
        {
            throw new ValidationException(
                "Please correct the highlighted fields.",
                $"MaxStores {input.MaxStores} < active stores {activeStores}.",
                new Dictionary<string, string[]>
                {
                    ["MaxStores"] = [$"Max stores cannot be below current usage ({activeStores} active stores)."]
                });
        }

        var updated = await _repo.UpdateLicenseAsync(input, ct);
        if (!updated.Ok)
        {
            throw new UpdateFailedException(
                updated.ResponseMsg ?? "Unable to save license.",
                updated.ResponseMsg ?? "Unable to save license.");
        }

        _cache.Remove(LicenseStatusCacheKey);

        await Audit(input.ActorUserId, "License", updated.IdLicense, "Update",
            $"License updated. Key={input.LicenseKey}, MaxStores={input.MaxStores}.", ct);
        return updated;
    }

    public async Task<OutputLicenseStatusUpdate> SetLicenseStatusAsync(
        InputLicenseStatusUpdate input,
        CancellationToken ct = default)
    {
        var license = await _repo.GetLicenseAsync(new InputLicenseSelect(), ct)
            ?? throw new ResourceNotFoundException("No license found.", "License row is missing.");

        var reason = input.Reason.Trim();
        var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC";
        var noteLine = input.Activate
            ? $"[{stamp}] Activated: {reason}"
            : $"[{stamp}] Suspended: {reason}";
        var notes = string.IsNullOrWhiteSpace(license.Notes)
            ? noteLine
            : license.Notes.Trim() + Environment.NewLine + noteLine;

        var updateInput = new InputLicenseUpdate
        {
            ActorUserId = input.ActorUserId,
            CanEditLicense = true,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseType,
            Status = input.Activate
                ? (int)CommonModel.LicenseStatus.Active
                : (int)CommonModel.LicenseStatus.Suspended,
            StartDate = license.StartDate,
            ExpiryDate = license.ExpiryDate,
            MaxStores = license.MaxStores,
            Notes = notes
        };

        var updated = await UpdateLicenseAsync(updateInput, ct);
        return new OutputLicenseStatusUpdate
        {
            ResponseCode = 0,
            ResponseMsg = updated.ResponseMsg,
            StatusCode = 200,
            IdLicense = updated.IdLicense,
            Status = updated.Status
        };
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

    private static void ThrowStoreOrCompanyFailure(int statusCode, string? message, string fallback)
    {
        var user = string.IsNullOrWhiteSpace(message) ? fallback : message;
        switch (statusCode)
        {
            case 404:
                throw new ResourceNotFoundException(user, user);
            case 409:
                throw new ConflictException(user, user);
            case 403:
                throw new ForbiddenException(user, user);
            default:
                throw new UpdateFailedException(user, user);
        }
    }
}

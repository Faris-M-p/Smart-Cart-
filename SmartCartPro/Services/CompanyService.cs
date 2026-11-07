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
    private readonly ISettingService _settings;

    public CompanyService(ICompanyRepository repo, IMemoryCache cache, ISettingService settings)
    {
        _repo = repo;
        _cache = cache;
        _settings = settings;
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
        if (existing is null || existing.IdCompany != input.IdCompany)
        {
            throw new ResourceNotFoundException(
                "Company not found.",
                $"Actor {input.ActorUserId} tried to update missing company {input.IdCompany}.");
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
        var today = await GetLicenseTodayAsync(ct);
        var licenses = await _repo.ListLicensesAsync(ct);
        var lic = licenses
            .Where(x => !x.Cancelled && x.StartDate <= today && x.ExpiryDate >= today)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault()
            ?? licenses.Where(x => !x.Cancelled && x.ExpiryDate < today)
                .OrderByDescending(x => x.ExpiryDate).FirstOrDefault()
            ?? licenses.Where(x => !x.Cancelled && x.StartDate > today)
                .OrderBy(x => x.StartDate).FirstOrDefault();
        if (lic is null) return null;
        SetEffectiveLicenseState(lic, today);
        lic.ActiveStoreCount = await _repo.GetActiveStoreCountAsync(ct);
        lic.ActiveUserCount = await _repo.GetActiveUserCountAsync(ct);
        return lic;
    }

    public async Task<List<OutputLicenseSelect>> ListLicensesAsync(CancellationToken ct = default)
    {
        var today = await GetLicenseTodayAsync(ct);
        var licenses = await _repo.ListLicensesAsync(ct);
        var stores = await _repo.GetActiveStoreCountAsync(ct);
        var users = await _repo.GetActiveUserCountAsync(ct);
        foreach (var license in licenses)
        {
            SetEffectiveLicenseState(license, today);
            license.ActiveStoreCount = stores;
            license.ActiveUserCount = users;
        }
        return licenses;
    }

    public async Task<OutputLicenseSelect?> GetLicenseByIdAsync(int idLicense, CancellationToken ct = default)
    {
        var license = await _repo.GetLicenseByIdAsync(idLicense, ct);
        if (license is null) return null;
        var today = await GetLicenseTodayAsync(ct);
        SetEffectiveLicenseState(license, today);
        license.ActiveStoreCount = await _repo.GetActiveStoreCountAsync(ct);
        license.ActiveUserCount = await _repo.GetActiveUserCountAsync(ct);
        return license;
    }

    public async Task<OutputLicenseUpdate> CreateLicenseAsync(InputLicenseCreate input, CancellationToken ct = default)
    {
        EnsureLicenseMutationAllowed(input.CanEditLicense, input.ActorUserId);
        ValidateLicenseDates(input.StartDate, input.ExpiryDate);
        await ValidateMaxStoresAsync(input.MaxStores, ct);
        await EnsureNoLicenseOverlapAsync(input.StartDate, input.ExpiryDate, null, ct);

        input.LicenseKey = "LIC-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
        var created = await _repo.CreateLicenseAsync(input, ct);
        if (!created.Ok)
        {
            await ThrowLicenseWriteFailureAsync(created, input.StartDate, input.ExpiryDate, null, ct);
        }

        await AuditLicenseAsync(input.ActorUserId, created.IdLicense, "Create",
            $"Before: none. After: key={created.LicenseKey}; start={created.StartDate:dd MMM yyyy}; expiry={created.ExpiryDate:dd MMM yyyy}; max stores={created.MaxStores}; notes={created.Notes ?? "Not provided"}. Reason: {input.Reason.Trim()}", ct);
        _cache.Remove(LicenseStatusCacheKey);
        return created;
    }

    public async Task<OutputLicenseUpdate> UpdateLicenseAsync(InputLicenseUpdate input, CancellationToken ct = default)
    {
        EnsureLicenseMutationAllowed(input.CanEditLicense, input.ActorUserId);
        ValidateLicenseDates(input.StartDate, input.ExpiryDate);
        await ValidateMaxStoresAsync(input.MaxStores, ct);
        var before = await GetLicenseByIdAsync(input.IdLicense, ct)
            ?? throw new ResourceNotFoundException("License not found.", $"License {input.IdLicense} does not exist.");
        if (before.Cancelled)
            throw new ConflictException("Cancelled licenses cannot be edited.", $"License {input.IdLicense} is cancelled.");
        await EnsureNoLicenseOverlapAsync(input.StartDate, input.ExpiryDate, input.IdLicense, ct);
        var updated = await _repo.UpdateLicenseAsync(input, ct);
        if (!updated.Ok)
        {
            await ThrowLicenseWriteFailureAsync(updated, input.StartDate, input.ExpiryDate, input.IdLicense, ct);
        }

        _cache.Remove(LicenseStatusCacheKey);
        await AuditLicenseAsync(input.ActorUserId, updated.IdLicense, "Update",
            $"Before: key={before.LicenseKey}; start={before.StartDate:dd MMM yyyy}; expiry={before.ExpiryDate:dd MMM yyyy}; max stores={before.MaxStores}; notes={before.Notes ?? "Not provided"}. After: key={updated.LicenseKey}; start={updated.StartDate:dd MMM yyyy}; expiry={updated.ExpiryDate:dd MMM yyyy}; max stores={updated.MaxStores}; notes={updated.Notes ?? "Not provided"}. Reason: {input.Reason.Trim()}", ct);
        return updated;
    }

    public async Task<OutputLicenseStatusUpdate> SetLicenseStatusAsync(
        InputLicenseStatusUpdate input,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Action)
            || input.Action is not ("suspend" or "resume" or "cancel"))
            throw new ValidationException("Please choose a valid license action.", "Unknown license action.");
        EnsureLicenseMutationAllowed(true, input.ActorUserId);
        input.Today = await GetLicenseTodayAsync(ct);
        var license = await GetLicenseByIdAsync(input.IdLicense, ct)
            ?? throw new ResourceNotFoundException("License not found.", $"License {input.IdLicense} does not exist.");
        if (license.Cancelled)
            throw new ConflictException("Cancelled licenses cannot be changed.", $"License {input.IdLicense} is cancelled.");
        if (input.Action == "resume" && !license.IsSuspended)
            throw new ConflictException("Only suspended licenses can be resumed.", $"License {input.IdLicense} is not suspended.");
        if (input.Action == "cancel" && license.StartDate <= license.Today)
            throw new ConflictException("Only a future license can be cancelled.", $"License {input.IdLicense} has already started.");

        var changed = await _repo.SetLicenseActionAsync(input, ct);
        if (!changed.Ok)
            ThrowLicenseStatusFailure(changed);
        _cache.Remove(LicenseStatusCacheKey);
        var actionName = input.Action switch { "resume" => "Resume", "cancel" => "Cancel", _ => "Suspend" };
        var resultingState = input.Action switch
        {
            "suspend" => "Suspended",
            "cancel" => "Cancelled",
            _ when license.StartDate > input.Today => "Scheduled",
            _ when license.ExpiryDate < input.Today => "Expired",
            _ => "Active"
        };
        await AuditLicenseAsync(input.ActorUserId, license.IdLicense, actionName,
            $"Before: status={license.StatusLabel}; start={license.StartDate:dd MMM yyyy}; expiry={license.ExpiryDate:dd MMM yyyy}; cancelled={license.Cancelled}. After: status={resultingState}; start={license.StartDate:dd MMM yyyy}; expiry={license.ExpiryDate:dd MMM yyyy}; cancelled={input.Action == "cancel"}. Reason: {input.Reason.Trim()}", ct);
        return changed;
    }

    public async Task<DateOnly> GetLicenseTodayAsync(CancellationToken ct = default)
    {
        var configuredTimeZone = await _settings.GetEffectiveValueAsync("locale.timezone", null, ct);
        return LocaleClock.LocalToday(LocaleClock.ResolveTimeZone(configuredTimeZone));
    }

    public Task<int> GetActiveStoreCountForLicenseAsync(CancellationToken ct = default) =>
        _repo.GetActiveStoreCountAsync(ct);

    public Task<int> GetActiveUserCountForLicenseAsync(CancellationToken ct = default) =>
        _repo.GetActiveUserCountAsync(ct);

    private static void SetEffectiveLicenseState(OutputLicenseSelect license, DateOnly today)
    {
        license.Today = today;
        license.Status = license.Cancelled
            ? (int)CommonModel.LicenseStatus.Cancelled
            : string.Equals(license.StoredStatus, "suspended", StringComparison.OrdinalIgnoreCase)
                ? (int)CommonModel.LicenseStatus.Suspended
                : today < license.StartDate
                    ? (int)CommonModel.LicenseStatus.Scheduled
                    : today > license.ExpiryDate
                        ? (int)CommonModel.LicenseStatus.Expired
                        : (int)CommonModel.LicenseStatus.Active;
    }

    private static void EnsureLicenseMutationAllowed(bool canEdit, int actorUserId)
    {
        if (!canEdit)
            throw new ForbiddenException("Only a platform administrator can change license settings.",
                $"Actor {actorUserId} cannot edit license records.");
    }

    private static void ValidateLicenseDates(DateOnly startDate, DateOnly expiryDate)
    {
        if (expiryDate >= startDate) return;
        const string message = "The Expiry Date must be on or after the Start Date.";
        throw new ValidationException("Please correct the highlighted fields.", message,
            new Dictionary<string, string[]> { ["ExpiryDate"] = [message] });
    }

    private async Task ValidateMaxStoresAsync(int maxStores, CancellationToken ct)
    {
        var activeStores = await _repo.GetActiveStoreCountAsync(ct);
        if (maxStores >= 1 && maxStores >= activeStores) return;
        var message = maxStores < 1
            ? "Max Stores must be at least 1."
            : $"Max stores cannot be below current usage ({activeStores} active stores).";
        throw new ValidationException("Please correct the highlighted fields.", message,
            new Dictionary<string, string[]> { ["MaxStores"] = [message] });
    }

    private async Task EnsureNoLicenseOverlapAsync(
        DateOnly startDate,
        DateOnly expiryDate,
        int? excludeId,
        CancellationToken ct)
    {
        var conflict = await _repo.FindLicenseOverlapAsync(startDate, expiryDate, excludeId, ct);
        if (conflict is not null)
            throw LicenseOverlapException(conflict);
    }

    private async Task ThrowLicenseWriteFailureAsync(
        OutputLicenseUpdate result,
        DateOnly startDate,
        DateOnly expiryDate,
        int? excludeId,
        CancellationToken ct)
    {
        if (result.StatusCode == 409)
        {
            var conflict = await _repo.FindLicenseOverlapAsync(startDate, expiryDate, excludeId, ct);
            if (conflict is not null) throw LicenseOverlapException(conflict);
            throw new ConflictException(result.ResponseMsg, result.ResponseMsg);
        }
        if (result.StatusCode == 404)
            throw new ResourceNotFoundException(result.ResponseMsg, result.ResponseMsg);
        throw new UpdateFailedException(result.ResponseMsg, result.ResponseMsg);
    }

    private static ConflictException LicenseOverlapException(OutputLicenseSelect conflict)
    {
        var message = $"A license already exists for this period: {conflict.LicenseKey} "
            + $"({conflict.StartDate:dd MMM yyyy} – {conflict.ExpiryDate:dd MMM yyyy}). "
            + "Edit that license or choose a non-overlapping period.";
        return new ConflictException(message, message);
    }

    private static void ThrowLicenseStatusFailure(OutputLicenseStatusUpdate result)
    {
        switch (result.StatusCode)
        {
            case 404:
                throw new ResourceNotFoundException(result.ResponseMsg, result.ResponseMsg);
            case 409:
                throw new ConflictException(result.ResponseMsg, result.ResponseMsg);
            default:
                throw new ValidationException(result.ResponseMsg, result.ResponseMsg);
        }
    }

    private Task AuditLicenseAsync(int actorUserId, int entityId, string action, string details, CancellationToken ct) =>
        _repo.AddAuditLogAsync(new InputAuditLogCreate
        {
            ActorUserId = actorUserId,
            EntityName = "License",
            EntityId = entityId,
            Action = action,
            Details = details
        }, ct);

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

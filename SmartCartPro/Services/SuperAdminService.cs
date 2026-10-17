using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class SuperAdminService : ISuperAdminService
{
    private readonly ISuperAdminRepository _repo;
    private readonly IEmailService _email;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ILogger<SuperAdminService> _logger;

    // Supported themes — must match DB CHECK constraint
    private static readonly string[] ValidThemes  = { "light", "dark" };
    private static readonly string[] ValidStatus  = { "active", "suspended", "expired", "trial" };
    private static readonly string[] ValidModes   = { "single", "multi" };

    public SuperAdminService(
        ISuperAdminRepository repo,
        IEmailService email,
        IPasswordHasher<User> hasher,
        ILogger<SuperAdminService> logger)
    {
        _repo   = repo;
        _email  = email;
        _hasher = hasher;
        _logger = logger;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<SuperAdminDashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var config       = await _repo.GetInstallationConfigAsync(ct);
        var license      = config?.License ?? await _repo.GetLicenseAsync(ct);
        var company      = await _repo.GetCompanyAsync(ct);
        var storeCount   = await _repo.GetActiveStoreCountAsync(ct);
        var moduleCount  = (await _repo.GetEnabledModuleIdsAsync(ct)).Count;
        var hasAdmin     = await _repo.HasCompanyAdminAsync(ct);

        var vm = new SuperAdminDashboardViewModel
        {
            OnboardingComplete = config?.OnboardingComplete ?? false,
            OnboardingStep     = config?.OnboardingStep ?? 0,
            CompanyName        = company?.CompanyName,
            Theme              = config?.Theme ?? "light",
            StoreMode          = config?.StoreMode ?? "single",
            CurrentStoreCount  = storeCount,
            MaxStores          = license?.MaxStores ?? 1,
            EnabledModuleCount = moduleCount,
            AdminProvisioned   = hasAdmin,
        };

        if (license is not null)
        {
            vm.LicenseStatus   = license.Status;
            vm.LicenseExpiry   = license.ExpiryDate;
            vm.DaysUntilExpiry = license.DaysUntilExpiry;
            vm.LicenseExpired  = license.Status != "active" || license.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);
            vm.LicenseSuspended = license.Status == "suspended";
            vm.LicenseNearExpiry = !vm.LicenseExpired && license.DaysUntilExpiry <= 30;
        }

        if (!vm.OnboardingComplete)
            vm.Warnings.Add("Setup is incomplete. Continue the Company Registration Wizard.");

        if (vm.LicenseExpired)
            vm.Warnings.Add("License has expired. Write operations are restricted.");
        else if (vm.LicenseSuspended)
            vm.Warnings.Add("License is suspended. Access is restricted.");
        else if (vm.LicenseNearExpiry)
            vm.Warnings.Add($"License expires in {vm.DaysUntilExpiry} day(s). Renew soon.");

        if (vm.OnboardingComplete && !vm.AdminProvisioned)
            vm.Warnings.Add("No Company Admin provisioned. Users cannot sign in.");

        // Recent activity
        var auditResult = await _repo.GetAuditLogsAsync(new AuditLogFilterInput { Page = 1, PageSize = 8 }, ct);
        vm.RecentActivity = auditResult.Rows.Select(a => new AuditLogRowViewModel
        {
            IdAuditLog = a.IdAuditLog,
            ActorName  = a.User?.FullName ?? "System",
            EntityName = a.EntityName,
            EntityId   = a.EntityId,
            Action     = a.Action,
            Details    = a.Details,
            CreatedAt  = a.CreatedAt
        }).ToList();

        return vm;
    }

    // ── Wizard Step 1 — Company Profile ───────────────────────────────────────

    public async Task<WizardStep1Model> GetStep1ModelAsync(CancellationToken ct = default)
    {
        var co = await _repo.GetCompanyAsync(ct);
        if (co is null) return new WizardStep1Model();
        return new WizardStep1Model
        {
            CompanyName    = co.CompanyName,
            CompanyCode    = co.CompanyCode,
            LegalName      = co.LegalName,
            TaxNumber      = co.TaxNumber,
            RegistrationNo = co.RegistrationNo,
            Email          = co.Email,
            Phone          = co.Phone,
            Website        = co.Website,
            AddressLine1   = co.AddressLine1,
            AddressLine2   = co.AddressLine2,
            City           = co.City,
            State          = co.State,
            PostalCode     = co.PostalCode,
            Country        = co.Country,
        };
    }

    public async Task<(bool Ok, string? Error)> SaveStep1Async(WizardStep1Model m, CancellationToken ct = default)
    {
        var co = new Company
        {
            CompanyName    = m.CompanyName.Trim(),
            CompanyCode    = m.CompanyCode.Trim().ToUpperInvariant(),
            LegalName      = m.LegalName?.Trim(),
            TaxNumber      = m.TaxNumber?.Trim(),
            RegistrationNo = m.RegistrationNo?.Trim(),
            Email          = m.Email?.Trim(),
            Phone          = m.Phone?.Trim(),
            Website        = m.Website?.Trim(),
            AddressLine1   = m.AddressLine1?.Trim(),
            AddressLine2   = m.AddressLine2?.Trim(),
            City           = m.City?.Trim(),
            State          = m.State?.Trim(),
            PostalCode     = m.PostalCode?.Trim(),
            Country        = m.Country?.Trim(),
        };
        await _repo.UpsertCompanyAsync(co, ct);
        await AdvanceStepAsync(1, ct);
        return (true, null);
    }

    // ── Wizard Step 2 — Regional ──────────────────────────────────────────────

    public async Task<WizardStep2Model> GetStep2ModelAsync(CancellationToken ct = default)
    {
        var co = await _repo.GetCompanyAsync(ct);
        return new WizardStep2Model
        {
            CurrencyCode = co?.CurrencyCode ?? "INR",
            TimeZone     = co?.TimeZone ?? "Asia/Kolkata",
            Locale       = co?.Locale ?? "en-IN",
        };
    }

    public async Task<(bool Ok, string? Error)> SaveStep2Async(WizardStep2Model m, CancellationToken ct = default)
    {
        var co = await _repo.GetCompanyAsync(ct) ?? new Company
        {
            CompanyName = "Unnamed", CompanyCode = "DEFAULT"
        };
        co.CurrencyCode = m.CurrencyCode.Trim().ToUpperInvariant();
        co.TimeZone     = m.TimeZone.Trim();
        co.Locale       = m.Locale.Trim();
        await _repo.UpsertCompanyAsync(co, ct);
        await AdvanceStepAsync(2, ct);
        return (true, null);
    }

    // ── Wizard Step 3 — Modules ───────────────────────────────────────────────

    public async Task<WizardStep3Model> GetStep3ModelAsync(CancellationToken ct = default)
    {
        var modules  = await _repo.GetAllActiveModulesAsync(ct);
        var presets  = await _repo.GetPresetsWithItemsAsync(ct);
        var selected = await _repo.GetEnabledModuleIdsAsync(ct);

        return new WizardStep3Model
        {
            SelectedPresetCode = "BASIC",
            SelectedModuleIds  = selected,
            AllModules         = modules.Select(m => new ModuleCatalogItem
            {
                IdModule     = m.IdModule,
                ModuleCode   = m.ModuleCode,
                ModuleName   = m.ModuleName,
                DisplayOrder = m.DisplayOrder,
            }).ToList(),
            Presets = presets.Select(p => new ModulePresetViewModel
            {
                IdPreset    = p.IdPreset,
                PresetCode  = p.PresetCode,
                PresetName  = p.PresetName,
                Description = p.Description,
                ModuleIds   = p.Items.Select(i => i.FkModule).ToList(),
            }).ToList(),
        };
    }

    public async Task<(bool Ok, string? Error)> SaveStep3Async(WizardStep3Model m, CancellationToken ct = default)
    {
        await _repo.SetEnabledModulesAsync(m.SelectedModuleIds, ct);
        await AdvanceStepAsync(3, ct);
        return (true, null);
    }

    // ── Wizard Step 4 — Theme ─────────────────────────────────────────────────

    public async Task<WizardStep4Model> GetStep4ModelAsync(CancellationToken ct = default)
    {
        var cfg = await _repo.GetInstallationConfigAsync(ct);
        return new WizardStep4Model { Theme = cfg?.Theme ?? "light" };
    }

    public async Task<(bool Ok, string? Error)> SaveStep4Async(WizardStep4Model m, CancellationToken ct = default)
    {
        if (!ValidThemes.Contains(m.Theme))
            return (false, "Invalid theme.");
        var cfg = await _repo.GetInstallationConfigAsync(ct) ?? new InstallationConfig();
        cfg.Theme = m.Theme;
        await _repo.UpsertInstallationConfigAsync(cfg, ct);
        await AdvanceStepAsync(4, ct);
        return (true, null);
    }

    // ── Wizard Step 5 — License ───────────────────────────────────────────────

    public async Task<WizardStep5Model> GetStep5ModelAsync(CancellationToken ct = default)
    {
        var lic = await _repo.GetLicenseAsync(ct);
        if (lic is null) return new WizardStep5Model();
        return new WizardStep5Model
        {
            LicenseKey       = lic.LicenseKey,
            LicenseType      = lic.LicenseType,
            LicenseStartDate = lic.StartDate,
            LicenseExpiryDate= lic.ExpiryDate,
            LicenseStatus    = lic.Status,
            MaxStores        = lic.MaxStores,
        };
    }

    public async Task<(bool Ok, string? Error)> SaveStep5Async(WizardStep5Model m, CancellationToken ct = default)
    {
        if (m.LicenseExpiryDate < m.LicenseStartDate)
            return (false, "Expiry date must be on or after the start date.");
        if (!ValidStatus.Contains(m.LicenseStatus))
            return (false, "Invalid license status.");
        if (m.MaxStores < 1)
            return (false, "Maximum stores must be at least 1.");

        var lic = new License
        {
            LicenseKey  = m.LicenseKey.Trim(),
            LicenseType = m.LicenseType.Trim(),
            Status      = m.LicenseStatus,
            StartDate   = m.LicenseStartDate,
            ExpiryDate  = m.LicenseExpiryDate,
            MaxStores   = m.MaxStores,
        };
        var saved = await _repo.UpsertLicenseAsync(lic, ct);

        var cfg = await _repo.GetInstallationConfigAsync(ct) ?? new InstallationConfig();
        cfg.FkLicense = saved.IdLicense;
        await _repo.UpsertInstallationConfigAsync(cfg, ct);
        await AdvanceStepAsync(5, ct);
        return (true, null);
    }

    // ── Wizard Step 6 — Store ─────────────────────────────────────────────────

    public async Task<WizardStep6Model> GetStep6ModelAsync(CancellationToken ct = default)
    {
        var store = await _repo.GetDefaultStoreAsync(ct);
        var lic   = await _repo.GetLicenseAsync(ct);
        if (store is null) return new WizardStep6Model { MaxStores = lic?.MaxStores ?? 1 };
        return new WizardStep6Model
        {
            StoreMode      = (await _repo.GetInstallationConfigAsync(ct))?.StoreMode ?? "single",
            StoreName      = store.StoreName,
            StoreCode      = store.StoreCode,
            StoreEmail     = store.Email,
            StorePhone     = store.Phone,
            StoreAddress   = store.AddressLine1,
            StoreCity      = store.City,
            StoreState     = store.State,
            StorePostalCode= store.PostalCode,
            StoreCountry   = store.Country,
            MaxStores      = lic?.MaxStores ?? 1,
        };
    }

    public async Task<(bool Ok, string? Error)> SaveStep6Async(WizardStep6Model m, CancellationToken ct = default)
    {
        if (!ValidModes.Contains(m.StoreMode))
            return (false, "Invalid store mode.");
        if (string.IsNullOrWhiteSpace(m.StoreName))
            return (false, "Store name is required.");
        if (string.IsNullOrWhiteSpace(m.StoreCode))
            return (false, "Store code is required.");

        var company = await _repo.GetCompanyAsync(ct);
        if (company is null)
            return (false, "Company profile must be saved before creating a store.");

        var existing = await _repo.GetDefaultStoreAsync(ct);
        if (existing is null)
        {
            var store = new Store
            {
                FkCompany    = company.IdCompany,
                StoreName    = m.StoreName.Trim(),
                StoreCode    = m.StoreCode.Trim().ToUpperInvariant(),
                Email        = m.StoreEmail?.Trim(),
                Phone        = m.StorePhone?.Trim(),
                AddressLine1 = m.StoreAddress?.Trim(),
                City         = m.StoreCity?.Trim(),
                State        = m.StoreState?.Trim(),
                PostalCode   = m.StorePostalCode?.Trim(),
                Country      = m.StoreCountry?.Trim(),
                IsDefault    = true,
                IsActive     = true,
            };
            await _repo.CreateStoreAsync(store, ct);
        }

        var cfg = await _repo.GetInstallationConfigAsync(ct) ?? new InstallationConfig();
        cfg.StoreMode = m.StoreMode;
        await _repo.UpsertInstallationConfigAsync(cfg, ct);
        await AdvanceStepAsync(6, ct);
        return (true, null);
    }

    // ── Wizard Step 7 — Admin Provisioning ───────────────────────────────────

    public async Task<(bool Ok, string? Error)> SaveStep7Async(
        int actorId, WizardStep7Model m, CancellationToken ct = default)
    {
        var email = m.AdminEmail.Trim().ToLowerInvariant();

        var existing = await _repo.GetUserByEmailAsync(email, ct);
        if (existing is not null && !existing.IsPlatformSuperAdmin)
            return (false, "A user with that email already exists.");

        var store = await _repo.GetDefaultStoreAsync(ct);
        if (store is null)
            return (false, "Store must be created (Step 6) before provisioning an admin.");

        var superAdminRole = await _repo.GetRoleByCodeAsync("SUPER_ADMIN", ct);
        if (superAdminRole is null)
            return (false, "SUPER_ADMIN role not found. Run BuildDatabase.bat seed scripts.");

        // Generate secure invitation token (48 hex chars)
        var token     = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var invitation = new UserInvitation
        {
            InvitedEmail    = email,
            InvitedFullName = m.AdminFullName.Trim(),
            InvitationToken = token,
            FkRole          = superAdminRole.IdRole,
            FkStore         = store.IdStore,
            Status          = "pending",
            ExpiresAt       = expiresAt,
            DeliveryStatus  = "pending",
        };
        var saved = await _repo.CreateInvitationAsync(invitation, ct);

        // Send invitation email
        bool emailSent = false;
        try
        {
            var setupUrl = $"/Auth/AcceptInvitation?token={Uri.EscapeDataString(token)}";
            await _email.SendAsync(
                email,
                "SmartCart Pro — You have been invited as Company Administrator",
                $"Hello {m.AdminFullName.Trim()},\n\n" +
                $"You have been provisioned as Company Administrator for SmartCart Pro.\n\n" +
                $"Click the link below to set your password and activate your account:\n" +
                $"{setupUrl}\n\n" +
                $"This invitation expires in 7 days.\n\n" +
                "If you did not expect this, please ignore it.",
                ct);
            emailSent = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver invitation email to {Email}", email);
        }

        saved.DeliveryStatus = emailSent ? "sent" : "failed";
        if (!emailSent)
            saved.DeliveryError = "SMTP delivery failed — see application logs.";
        await _repo.UpdateInvitationAsync(saved, ct);

        await _repo.AddAuditLogAsync(new AuditLog
        {
            FkUser     = actorId,
            EntityName = "UserInvitation",
            EntityId   = saved.IdInvitation,
            Action     = "Create",
            Details    = $"Invitation created for {email} as SUPER_ADMIN. Delivery: {saved.DeliveryStatus}.",
        }, ct);

        await AdvanceStepAsync(7, ct);
        return (true, emailSent ? null : "Invitation created but email delivery failed. Check SMTP settings.");
    }

    // ── Review ────────────────────────────────────────────────────────────────

    public async Task<WizardReviewModel> GetReviewModelAsync(CancellationToken ct = default)
    {
        var co      = await _repo.GetCompanyAsync(ct);
        var lic     = await _repo.GetLicenseAsync(ct);
        var cfg     = await _repo.GetInstallationConfigAsync(ct);
        var store   = await _repo.GetDefaultStoreAsync(ct);
        var modIds  = await _repo.GetEnabledModuleIdsAsync(ct);
        var allMods = await _repo.GetAllActiveModulesAsync(ct);
        var inv     = await _repo.GetLatestInvitationAsync(ct);

        var draft = new OnboardingDraft
        {
            CompanyName      = co?.CompanyName ?? "",
            CompanyCode      = co?.CompanyCode ?? "",
            LegalName        = co?.LegalName,
            Email            = co?.Email,
            Phone            = co?.Phone,
            CurrencyCode     = co?.CurrencyCode ?? "INR",
            TimeZone         = co?.TimeZone ?? "Asia/Kolkata",
            Locale           = co?.Locale ?? "en-IN",
            Theme            = cfg?.Theme ?? "light",
            StoreMode        = cfg?.StoreMode ?? "single",
            LicenseKey       = lic?.LicenseKey ?? "",
            LicenseType      = lic?.LicenseType ?? "",
            LicenseStatus    = lic?.Status ?? "",
            LicenseStartDate = lic?.StartDate ?? DateOnly.MinValue,
            LicenseExpiryDate= lic?.ExpiryDate ?? DateOnly.MinValue,
            MaxStores        = lic?.MaxStores ?? 1,
            StoreName        = store?.StoreName ?? "",
            StoreCode        = store?.StoreCode ?? "",
            AdminEmail       = inv?.InvitedEmail ?? "",
            AdminFullName    = inv?.InvitedFullName ?? "",
            SelectedModuleIds= modIds,
        };

        var moduleNames = allMods
            .Where(m => modIds.Contains(m.IdModule))
            .Select(m => m.ModuleName)
            .ToList();

        return new WizardReviewModel { Draft = draft, SelectedModuleNames = moduleNames };
    }

    // ── Activate ──────────────────────────────────────────────────────────────

    public async Task<(bool Ok, string? Error)> ActivateAsync(int actorId, CancellationToken ct = default)
    {
        var co    = await _repo.GetCompanyAsync(ct);
        var lic   = await _repo.GetLicenseAsync(ct);
        var store = await _repo.GetDefaultStoreAsync(ct);
        var mods  = await _repo.GetEnabledModuleIdsAsync(ct);

        if (co is null)        return (false, "Company profile is not saved.");
        if (lic is null)       return (false, "License is not configured.");
        if (store is null)     return (false, "Initial store is not created.");
        if (mods.Count == 0)   return (false, "At least one module must be enabled.");
        if (lic.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return (false, "License expiry date is in the past.");

        var cfg = await _repo.GetInstallationConfigAsync(ct) ?? new InstallationConfig();
        cfg.OnboardingComplete = true;
        cfg.OnboardingStep     = 8;
        cfg.ActivatedAt        = DateTime.UtcNow;
        cfg.FkLicense          = lic.IdLicense;
        await _repo.UpsertInstallationConfigAsync(cfg, ct);

        await _repo.AddAuditLogAsync(new AuditLog
        {
            FkUser     = actorId,
            EntityName = "Installation",
            Action     = "Activate",
            Details    = $"Installation activated. Company: {co.CompanyName}. License: {lic.LicenseKey}.",
        }, ct);

        _logger.LogInformation("Installation activated by user {UserId}. Company: {Company}", actorId, co.CompanyName);
        return (true, null);
    }

    // ── License Management ────────────────────────────────────────────────────

    public async Task<LicenseViewModel?> GetLicenseViewModelAsync(CancellationToken ct = default)
    {
        var lic = await _repo.GetLicenseAsync(ct);
        if (lic is null) return null;
        return new LicenseViewModel
        {
            IdLicense   = lic.IdLicense,
            LicenseKey  = lic.LicenseKey,
            LicenseType = lic.LicenseType,
            Status      = lic.Status,
            StartDate   = lic.StartDate,
            ExpiryDate  = lic.ExpiryDate,
            MaxStores   = lic.MaxStores,
            Notes       = lic.Notes,
        };
    }

    public async Task<(bool Ok, string? Error)> UpdateLicenseAsync(
        int actorId, EditLicenseInput m, CancellationToken ct = default)
    {
        if (m.ExpiryDate < m.StartDate)
            return (false, "Expiry must be on or after start date.");
        if (!ValidStatus.Contains(m.Status))
            return (false, "Invalid status.");

        var lic = new License
        {
            LicenseKey  = m.LicenseKey.Trim(),
            LicenseType = m.LicenseType.Trim(),
            Status      = m.Status,
            StartDate   = m.StartDate,
            ExpiryDate  = m.ExpiryDate,
            MaxStores   = m.MaxStores,
            Notes       = m.Notes?.Trim(),
        };
        await _repo.UpsertLicenseAsync(lic, ct);

        await _repo.AddAuditLogAsync(new AuditLog
        {
            FkUser     = actorId,
            EntityName = "License",
            Action     = "Update",
            Details    = $"License updated. Key={m.LicenseKey}, Status={m.Status}, Expiry={m.ExpiryDate}.",
        }, ct);

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> SuspendLicenseAsync(int actorId, CancellationToken ct = default)
    {
        var lic = await _repo.GetLicenseAsync(ct);
        if (lic is null) return (false, "No license found.");
        if (lic.Status == "suspended") return (false, "License is already suspended.");
        lic.Status = "suspended";
        await _repo.UpsertLicenseAsync(lic, ct);

        await _repo.AddAuditLogAsync(new AuditLog
        {
            FkUser     = actorId,
            EntityName = "License",
            Action     = "Suspend",
            Details    = $"License {lic.LicenseKey} suspended.",
        }, ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> ReactivateLicenseAsync(int actorId, CancellationToken ct = default)
    {
        var lic = await _repo.GetLicenseAsync(ct);
        if (lic is null) return (false, "No license found.");
        if (lic.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return (false, "Cannot reactivate an expired license. Update the expiry date first.");
        lic.Status = "active";
        await _repo.UpsertLicenseAsync(lic, ct);

        await _repo.AddAuditLogAsync(new AuditLog
        {
            FkUser     = actorId,
            EntityName = "License",
            Action     = "Reactivate",
            Details    = $"License {lic.LicenseKey} reactivated.",
        }, ct);
        return (true, null);
    }

    // ── Audit Log ─────────────────────────────────────────────────────────────

    public async Task<AuditLogListViewModel> GetAuditLogsAsync(AuditLogFilterInput filter, CancellationToken ct = default)
    {
        filter.Page     = Math.Max(1, filter.Page);
        filter.PageSize = Math.Clamp(filter.PageSize, 10, 200);

        var (rows, total) = await _repo.GetAuditLogsAsync(filter, ct);

        return new AuditLogListViewModel
        {
            Filter     = filter,
            TotalCount = total,
            Rows       = rows.Select(a => new AuditLogRowViewModel
            {
                IdAuditLog = a.IdAuditLog,
                ActorName  = a.User?.FullName ?? "System",
                EntityName = a.EntityName,
                EntityId   = a.EntityId,
                Action     = a.Action,
                Details    = a.Details,
                CreatedAt  = a.CreatedAt,
            }).ToList(),
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task AdvanceStepAsync(int step, CancellationToken ct)
    {
        var cfg = await _repo.GetInstallationConfigAsync(ct) ?? new InstallationConfig();
        if (cfg.OnboardingStep < step)
            cfg.OnboardingStep = step;
        await _repo.UpsertInstallationConfigAsync(cfg, ct);
    }
}

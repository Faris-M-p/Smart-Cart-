using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ISuperAdminService
{
    Task<SuperAdminDashboardViewModel> GetDashboardAsync(CancellationToken ct = default);

    // Onboarding wizard — each step saves to DB immediately so progress survives restarts
    Task<(bool Ok, string? Error)> SaveStep1Async(WizardStep1Model input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveStep2Async(WizardStep2Model input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveStep3Async(WizardStep3Model input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveStep4Async(WizardStep4Model input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveStep5Async(WizardStep5Model input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveStep6Async(WizardStep6Model input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveStep7Async(int actorId, WizardStep7Model input, CancellationToken ct = default);

    Task<WizardReviewModel> GetReviewModelAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> ActivateAsync(int actorId, CancellationToken ct = default);

    // Pre-fill wizard steps from current DB state
    Task<WizardStep1Model> GetStep1ModelAsync(CancellationToken ct = default);
    Task<WizardStep2Model> GetStep2ModelAsync(CancellationToken ct = default);
    Task<WizardStep3Model> GetStep3ModelAsync(CancellationToken ct = default);
    Task<WizardStep4Model> GetStep4ModelAsync(CancellationToken ct = default);
    Task<WizardStep5Model> GetStep5ModelAsync(CancellationToken ct = default);
    Task<WizardStep6Model> GetStep6ModelAsync(CancellationToken ct = default);

    // License management
    Task<LicenseViewModel?> GetLicenseViewModelAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateLicenseAsync(int actorId, EditLicenseInput input, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SuspendLicenseAsync(int actorId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> ReactivateLicenseAsync(int actorId, CancellationToken ct = default);

    // Audit log
    Task<AuditLogListViewModel> GetAuditLogsAsync(AuditLogFilterInput filter, CancellationToken ct = default);
}

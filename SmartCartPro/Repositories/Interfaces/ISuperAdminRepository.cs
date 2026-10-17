using SmartCartPro.Entities;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ISuperAdminRepository
{
    // Company
    Task<Company?> GetCompanyAsync(CancellationToken ct = default);
    Task<Company> UpsertCompanyAsync(Company company, CancellationToken ct = default);

    // Store
    Task<Store?> GetDefaultStoreAsync(CancellationToken ct = default);
    Task<int> GetActiveStoreCountAsync(CancellationToken ct = default);
    Task<Store> CreateStoreAsync(Store store, CancellationToken ct = default);

    // License
    Task<License?> GetLicenseAsync(CancellationToken ct = default);
    Task<License> UpsertLicenseAsync(License license, CancellationToken ct = default);

    // Installation config
    Task<InstallationConfig?> GetInstallationConfigAsync(CancellationToken ct = default);
    Task<InstallationConfig> UpsertInstallationConfigAsync(InstallationConfig config, CancellationToken ct = default);

    // Modules
    Task<List<Module>> GetAllActiveModulesAsync(CancellationToken ct = default);
    Task<List<ModulePreset>> GetPresetsWithItemsAsync(CancellationToken ct = default);
    Task<List<int>> GetEnabledModuleIdsAsync(CancellationToken ct = default);
    Task SetEnabledModulesAsync(IEnumerable<int> moduleIds, CancellationToken ct = default);

    // Roles
    Task<Role?> GetRoleByCodeAsync(string roleCode, CancellationToken ct = default);

    // Users
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task<User> CreateUserAsync(User user, CancellationToken ct = default);
    Task AssignRoleAsync(int idUser, int idRole, CancellationToken ct = default);
    Task GrantStoreAccessAsync(int idUser, int idStore, bool isDefault, CancellationToken ct = default);
    Task<bool> HasCompanyAdminAsync(CancellationToken ct = default);

    // Invitations
    Task<UserInvitation> CreateInvitationAsync(UserInvitation invitation, CancellationToken ct = default);
    Task<UserInvitation?> GetInvitationByTokenAsync(string token, CancellationToken ct = default);
    Task UpdateInvitationAsync(UserInvitation invitation, CancellationToken ct = default);
    Task<UserInvitation?> GetLatestInvitationAsync(CancellationToken ct = default);

    // Audit log
    Task AddAuditLogAsync(AuditLog log, CancellationToken ct = default);
    Task<(List<AuditLog> Rows, int TotalCount)> GetAuditLogsAsync(
        AuditLogFilterInput filter, CancellationToken ct = default);
}

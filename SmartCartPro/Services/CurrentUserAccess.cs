using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>Per-request cache of the signed-in user's permission codes and the installation's enabled modules.</summary>
public sealed class CurrentUserAccess : ICurrentUserAccess
{
    /// <summary>Company administrator role. Shop modules still follow the installation; these two do not.</summary>
    private const string CompanyAdminRoleCode = "ADMIN";

    private static readonly HashSet<string> CompanyAdminModules = new(StringComparer.OrdinalIgnoreCase)
    {
        "USERS",
        "SETTINGS"
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly IModuleService _moduleService;

    private HashSet<string>? _permissions;
    private OutputFeatureGate? _gate;
    private bool? _isCompanyAdmin;

    public CurrentUserAccess(
        IHttpContextAccessor httpContextAccessor,
        IUserService userService,
        IModuleService moduleService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _moduleService = moduleService;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsPlatformSuperAdmin =>
        AuthClaims.IsPlatformSuperAdminUser(_httpContextAccessor.HttpContext?.User);

    public async Task<bool> CanAsync(string permissionCode, CancellationToken ct = default)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        if (IsPlatformSuperAdmin)
        {
            return true;
        }

        var permissions = await LoadPermissionsAsync(ct);
        return permissions.Contains(permissionCode);
    }

    public async Task<bool> CanAnyAsync(IEnumerable<string> permissionCodes, CancellationToken ct = default)
    {
        foreach (var code in permissionCodes)
        {
            if (await CanAsync(code, ct))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> ModuleEnabledAsync(string moduleCode, CancellationToken ct = default)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        if (IsPlatformSuperAdmin)
        {
            return true;
        }

        // Employees, roles, company, stores, settings, and audit stay available to the company Admin
        // even when the software provider left Users and Settings off the module preset.
        if (CompanyAdminModules.Contains(moduleCode) && await IsCompanyAdminAsync(ct))
        {
            return true;
        }

        var gate = await LoadGateAsync(ct);
        return gate.EnabledModuleCodes.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> FeatureEnabledAsync(string featureCode, CancellationToken ct = default)
    {
        if (!IsAuthenticated || string.IsNullOrWhiteSpace(featureCode))
        {
            return false;
        }

        if (IsPlatformSuperAdmin)
        {
            return true;
        }

        var gate = await LoadGateAsync(ct);
        if (!gate.CatalogReady)
        {
            var module = featureCode.Split('.')[0];
            return await ModuleEnabledAsync(module, ct);
        }

        if (gate.CompanyAdminCoreFeatureCodes.Contains(featureCode, StringComparer.OrdinalIgnoreCase)
            && await IsCompanyAdminAsync(ct))
        {
            return true;
        }

        if (!gate.EnabledFeatureCodes.Contains(featureCode, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var moduleCode = featureCode.Split('.')[0];
        return await ModuleEnabledAsync(moduleCode, ct);
    }

    public async Task<bool> AnyFeatureEnabledAsync(IEnumerable<string> featureCodes, CancellationToken ct = default)
    {
        foreach (var code in featureCodes)
        {
            if (await FeatureEnabledAsync(code, ct))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<OutputFeatureGate> LoadGateAsync(CancellationToken ct)
    {
        if (_gate is not null)
        {
            return _gate;
        }

        _gate = await _moduleService.GetFeatureGateAsync(ct);
        return _gate;
    }

    private async Task<HashSet<string>> LoadPermissionsAsync(CancellationToken ct)
    {
        if (_permissions is not null)
        {
            return _permissions;
        }

        var userId = AuthClaims.GetUserId(_httpContextAccessor.HttpContext?.User);
        if (userId is null or 0)
        {
            return _permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var rows = await _userService.GetUserPermissionsAsync(userId.Value, ct);
        _permissions = new HashSet<string>(
            rows.Select(r => r.PermissionCode),
            StringComparer.OrdinalIgnoreCase);
        return _permissions;
    }

    private async Task<bool> IsCompanyAdminAsync(CancellationToken ct)
    {
        if (_isCompanyAdmin is not null)
        {
            return _isCompanyAdmin.Value;
        }

        var userId = AuthClaims.GetUserId(_httpContextAccessor.HttpContext?.User);
        if (userId is null or 0)
        {
            _isCompanyAdmin = false;
            return false;
        }

        var hasRole = await _userService.UserHasRoleAsync(userId.Value, CompanyAdminRoleCode, ct);
        _isCompanyAdmin = hasRole;
        return hasRole;
    }
}

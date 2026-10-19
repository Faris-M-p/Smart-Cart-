using SmartCartPro.Helpers;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>Per-request cache of the signed-in user's permission codes and the installation's enabled modules.</summary>
public sealed class CurrentUserAccess : ICurrentUserAccess
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly IModuleService _moduleService;

    private HashSet<string>? _permissions;
    private HashSet<string>? _enabledModules;

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

        if (_enabledModules is null)
        {
            var codes = await _moduleService.GetEnabledModuleCodesAsync(ct);
            _enabledModules = new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
        }

        return _enabledModules.Contains(moduleCode);
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
}

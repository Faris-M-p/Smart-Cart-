namespace SmartCartPro.Services.Interfaces;

/// <summary>
/// What the signed-in user may see/do, for Razor views (menu items, action buttons).
/// Loaded lazily once per request. Platform Super Admin passes every check.
/// Server-side enforcement still happens in <c>[RequirePermission]</c> / <c>[ModuleAccess]</c>; this only shapes the UI.
/// </summary>
public interface ICurrentUserAccess
{
    bool IsAuthenticated { get; }

    /// <summary>True for the platform Super Admin claim (no store, bypasses permission checks).</summary>
    bool IsPlatformSuperAdmin { get; }

    /// <summary>True when the user holds the permission code (e.g. <c>products.manage</c>).</summary>
    Task<bool> CanAsync(string permissionCode, CancellationToken ct = default);

    /// <summary>True when the user holds at least one of the permission codes.</summary>
    Task<bool> CanAnyAsync(IEnumerable<string> permissionCodes, CancellationToken ct = default);

    /// <summary>True when the module (e.g. <c>PRODUCTS</c>) is enabled for this installation. Users and Settings are also true for the company Admin role.</summary>
    Task<bool> ModuleEnabledAsync(string moduleCode, CancellationToken ct = default);
}

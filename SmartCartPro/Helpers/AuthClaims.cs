using System.Security.Claims;

namespace SmartCartPro.Helpers;

/// <summary>JWT cookie and claim names for SmartCartPro admin auth. Store claim is <c>fk_store</c>.</summary>
public static class AuthClaims
{
    public const string TokenCookieName = "smartcartpro.auth.token";

    public const string Id_User = "id_user";
    public const string Fk_Store = "fk_store";
    public const string Fk_Company = "fk_company";
    public const string StoreCode = "store_code";
    public const string StoreName = "store_name";
    public const string MustChangePassword = "must_change_password";
    public const string IsPlatformSuperAdmin = "is_platform_super_admin";
    public const string RememberMe = "remember_me";
    public const string SessionVersion = "session_version";

    public static int? GetUserId(ClaimsPrincipal? user)
    {
        var value = user?.FindFirstValue(Id_User) ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static int? GetFkStore(ClaimsPrincipal? user)
    {
        var value = user?.FindFirstValue(Fk_Store);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static int? GetFkCompany(ClaimsPrincipal? user)
    {
        var value = user?.FindFirstValue(Fk_Company);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static string? GetStoreName(ClaimsPrincipal? user) =>
        user?.FindFirstValue(StoreName);

    public static bool IsPlatformSuperAdminUser(ClaimsPrincipal? user) =>
        user?.FindFirstValue(IsPlatformSuperAdmin) == "1";

    public static int GetSessionVersion(ClaimsPrincipal? user)
    {
        var value = user?.FindFirstValue(SessionVersion);
        return int.TryParse(value, out var version) ? version : 0;
    }

    /// <summary>True when the original sign-in used Remember me (persistent cookie + long-lived JWT).</summary>
    public static bool WantsPersistentSession(ClaimsPrincipal? user) =>
        user?.FindFirstValue(RememberMe) == "1";
}

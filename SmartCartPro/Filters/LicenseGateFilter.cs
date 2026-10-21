using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Global action filter that enforces license constraints on every request:
/// <list type="bullet">
///   <item>Suspended license → 403 for all requests.</item>
///   <item>Expired license → 403 for POST/PUT/DELETE; GET allowed (read-only mode).</item>
///   <item>Platform super admin always bypasses.</item>
/// </list>
/// Register with <c>options.Filters.AddService&lt;LicenseGateFilter&gt;()</c> in Program.cs.
/// </summary>
public sealed class LicenseGateFilter : IAsyncActionFilter
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ICompanyService _companySvc;
    private readonly IMemoryCache _cache;

    public LicenseGateFilter(ICompanyService companySvc, IMemoryCache cache)
    {
        _companySvc = companySvc;
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Platform super admin is never gated.
        if (AuthClaims.IsPlatformSuperAdminUser(context.HttpContext.User))
        {
            await next();
            return;
        }

        // Anonymous endpoints (sign-in, OTP, password reset, error and license pages) must stay reachable;
        // otherwise a suspended/expired license would lock everyone — including the Super Admin who fixes it — out.
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            await next();
            return;
        }

        var license = await GetLicenseAsync(context.HttpContext.RequestAborted);

        if (license is null)
        {
            // No license at all — allow read-only GET only.
            if (!IsReadOnly(context))
            {
                Block(context, "none", "No active license. Contact your administrator.");
            }
            else
            {
                await next();
            }
            return;
        }

        if (license.Status == (int)CommonModel.LicenseStatus.Suspended)
        {
            Block(context, "suspended", "License is suspended. Contact your administrator.");
            return;
        }

        if (license.IsExpired && !IsReadOnly(context))
        {
            Block(context, "expired", "License has expired. Data entry is disabled. Contact your administrator.");
            return;
        }

        await next();
    }

    /// <summary>AJAX callers get the common JSON envelope (403); page navigations get the friendly license page.</summary>
    private static void Block(ActionExecutingContext context, string reason, string message)
    {
        if (ExceptionErrorMapper.WantsJsonResponse(context.HttpContext.Request))
        {
            context.Result = new ObjectResult(CommonResponseModel.Failure(message, statusCode: 403))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        context.Result = new RedirectToActionResult("LicenseBlocked", "Home", new { reason });
    }

    private async Task<OutputLicenseSelect?> GetLicenseAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue<OutputLicenseSelect>(CompanyService.LicenseStatusCacheKey, out var cached))
        {
            return cached;
        }

        var lic = await _companySvc.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (lic is not null)
        {
            _cache.Set(CompanyService.LicenseStatusCacheKey, lic, CacheTtl);
        }

        return lic;
    }

    private static bool IsReadOnly(ActionExecutingContext ctx)
    {
        var method = ctx.HttpContext.Request.Method;
        return string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase);
    }
}

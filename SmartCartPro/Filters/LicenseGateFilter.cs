using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
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
    private const string CacheKey = "smartcart:license:status";
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

        var license = await GetLicenseAsync(context.HttpContext.RequestAborted);

        if (license is null)
        {
            // No license at all — allow only super admin (already handled) or read-only GET.
            if (!IsReadOnly(context))
            {
                context.Result = new ObjectResult("No active license. Contact your administrator.")
                {
                    StatusCode = 403
                };
            }
            else
            {
                await next();
            }
            return;
        }

        if (license.Status == "suspended")
        {
            context.Result = new ObjectResult("License is suspended. Contact your administrator.")
            {
                StatusCode = 403
            };
            return;
        }

        if (license.IsExpired && !IsReadOnly(context))
        {
            context.Result = new ObjectResult("License has expired. Data entry is disabled. Contact your administrator.")
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }

    private async Task<OutputLicenseSelect?> GetLicenseAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue<OutputLicenseSelect>(CacheKey, out var cached))
        {
            return cached;
        }

        var lic = await _companySvc.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (lic is not null)
        {
            _cache.Set(CacheKey, lic, CacheTtl);
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

using System.Security.Claims;
using SmartCart.Application.Abstractions.Security;
using SmartCart.Application.Abstractions.Tenant;

namespace SmartCart.API.Middleware;

/// <summary>
/// Binds tenant from authenticated claims only. Never trusts CompanyId / database name from the request body.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (tenantContext is TenantContext mutable)
        {
            var tenant = context.User.FindFirstValue(AuthClaimTypes.TenantId);
            var code = context.User.FindFirstValue(AuthClaimTypes.CompanyCode);
            if (Guid.TryParse(tenant, out var id) && id != Guid.Empty)
            {
                mutable.CompanyAccountId = id;
                mutable.CompanyCode = code;
            }
        }

        await _next(context);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCart.Application.Abstractions.Security;
using SmartCart.Application.Abstractions.Tenant;

namespace SmartCart.API.Controllers.Platform;

[ApiController]
[Route("api/platform/license")]
[Authorize]
public sealed class LicenseController : ControllerBase
{
    private readonly ILicenseService _licenseService;
    private readonly ITenantContext _tenant;

    public LicenseController(ILicenseService licenseService, ITenantContext tenant)
    {
        _licenseService = licenseService;
        _tenant = tenant;
    }

    /// <summary>Returns license snapshot for the authenticated tenant (no connection secrets).</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!_tenant.IsResolved)
        {
            return Unauthorized(new { message = "Tenant context is missing from the token." });
        }

        var snap = await _licenseService.GetSnapshotAsync(_tenant.CompanyAccountId!.Value, cancellationToken);
        return Ok(snap);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCart.Infrastructure.Persistence;

namespace SmartCart.API.Controllers.Storefront;

/// <summary>
/// Public storefront configuration. Tenant is resolved from a trusted host/header mapping later;
/// Phase 2 exposes a safe placeholder that never returns DB credentials.
/// </summary>
[ApiController]
[Route("api/storefront/config")]
[AllowAnonymous]
public sealed class StorefrontConfigController : ControllerBase
{
    private readonly PlatformDbContext _platformDb;

    public StorefrontConfigController(PlatformDbContext platformDb)
    {
        _platformDb = platformDb;
    }

    [HttpGet("{companyCode}")]
    public async Task<IActionResult> Get(string companyCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
        {
            return BadRequest(new { message = "Company code is required." });
        }

        var company = await _platformDb.CompanyAccounts
            .AsNoTracking()
            .Include(c => c.Theme!)
            .ThenInclude(t => t.Theme)
            .SingleOrDefaultAsync(c => c.Code == companyCode, cancellationToken);

        if (company is null || company.Status != "Ready")
        {
            return NotFound(new { message = "Storefront is not available." });
        }

        return Ok(new
        {
            companyCode = company.Code,
            companyName = company.Name,
            themeCode = company.Theme?.Theme.Code ?? "default",
            themeVariables = company.Theme?.Theme.CssVariablesJson ?? "{}"
            // Intentionally omits connection strings and other secrets.
        });
    }
}

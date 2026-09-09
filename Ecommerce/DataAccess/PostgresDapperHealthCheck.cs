using Ecommerce.Interface;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecommerce.DataAccess
{
    /// <summary>
    /// Health check that verifies PostgreSQL connectivity via <see cref="IDataAccessDapper.TestConnectionAsync"/>.
    /// </summary>
    public sealed class PostgresDapperHealthCheck : IHealthCheck
    {
        private readonly IDataAccessDapper _dapper;

        public PostgresDapperHealthCheck(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var ok = await _dapper.TestConnectionAsync(cancellationToken);
            return ok
                ? HealthCheckResult.Healthy("PostgreSQL connection OK.")
                : HealthCheckResult.Unhealthy("PostgreSQL connection failed.");
        }
    }
}

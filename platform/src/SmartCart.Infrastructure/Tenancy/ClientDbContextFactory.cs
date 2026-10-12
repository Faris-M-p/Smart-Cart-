using Microsoft.EntityFrameworkCore;
using SmartCart.Application.Abstractions.Tenant;
using SmartCart.Infrastructure.Persistence;

namespace SmartCart.Infrastructure.Tenancy;

public interface IClientDbContextFactory
{
    Task<ClientDbContext> CreateAsync(Guid companyAccountId, CancellationToken cancellationToken = default);
}

public sealed class ClientDbContextFactory : IClientDbContextFactory
{
    private readonly ITenantConnectionResolver _resolver;

    public ClientDbContextFactory(ITenantConnectionResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<ClientDbContext> CreateAsync(
        Guid companyAccountId,
        CancellationToken cancellationToken = default)
    {
        var cs = await _resolver.GetClientConnectionStringAsync(companyAccountId, cancellationToken);
        var options = new DbContextOptionsBuilder<ClientDbContext>()
            .UseNpgsql(cs)
            .UseSnakeCaseNamingConvention()
            .Options;
        return new ClientDbContext(options);
    }
}

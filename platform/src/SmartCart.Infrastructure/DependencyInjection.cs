using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCart.Application.Abstractions.Security;
using SmartCart.Application.Abstractions.Tenant;
using SmartCart.Infrastructure.Persistence;
using SmartCart.Infrastructure.Security;
using SmartCart.Infrastructure.Tenancy;

namespace SmartCart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSmartCartInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDataProtection();

        var platformCs = configuration.GetConnectionString("PlatformDatabase")
            ?? configuration["ConnectionStrings:PlatformDatabase"]
            ?? "Host=localhost;Database=smartcart_platform;Username=postgres;Password=postgres";

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(platformCs).UseSnakeCaseNamingConvention());

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ITenantConnectionResolver, TenantConnectionResolver>();
        services.AddScoped<IClientDbContextFactory, ClientDbContextFactory>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IStoreAccessService, StoreAccessService>();

        return services;
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCartPro.Configuration;
using SmartCartPro.Dapper;
using SmartCartPro.Dapper.Interfaces;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Middleware;
using SmartCartPro.Repositories;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartCartExceptionHandling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EnvironmentSettings>(configuration.GetSection(EnvironmentSettings.SectionName));
        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IServiceCollection AddSmartCartDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=smartcart_pro;Username=postgres;Password=CHANGE_ME";

        services.AddDbContext<SmartCartDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories (EF Core CRUD)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Optional Dapper (reports / justified queries only)
        services.AddScoped<IProductReportRepository, ProductReportRepository>();

        // Common filter / dropdown fills (EF LINQ)
        services.AddScoped<IFilterRepository, FilterRepository>();
        services.AddScoped<IFilterService, FilterService>();

        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }

    public static IServiceCollection AddSmartCartAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<OtpSettings>(configuration.GetSection(OtpSettings.SectionName));
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<AuthJwtTokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
        services.AddScoped<ISuperAdminService, SuperAdminService>();
        return services;
    }

    /// <summary>
    /// Registers the <c>PlatformSuperAdmin</c> authorization policy used to
    /// protect every Super Admin controller action.
    /// </summary>
    public static IServiceCollection AddSmartCartPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("PlatformSuperAdmin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(SmartCartPro.Helpers.AuthClaims.IsPlatformSuperAdmin, "1"));
        });
        return services;
    }
}

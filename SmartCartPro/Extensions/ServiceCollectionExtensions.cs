using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCartPro.Configuration;
using SmartCartPro.Dapper;
using SmartCartPro.Dapper.Interfaces;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Filters;
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
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyService, CompanyService>();

        // Module management
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IModuleService, ModuleService>();

        // User management
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        // Role management
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();

        // Document numbering
        services.AddScoped<IDocumentNumberService, DocumentNumberService>();

        return services;
    }

    public static IServiceCollection AddSmartCartFilters(this IServiceCollection services)
    {
        // LicenseGateFilter is registered as scoped so it can use ICompanyService.
        // IMemoryCache (AddMemoryCache) must be registered before calling this.
        services.AddScoped<LicenseGateFilter>();
        return services;
    }

    /// <summary>
    /// Platform claim policy — store create / company bootstrap.
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

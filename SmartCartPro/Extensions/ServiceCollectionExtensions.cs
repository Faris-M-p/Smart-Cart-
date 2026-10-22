using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCartPro.Configuration;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Filters;
using SmartCartPro.Middleware;
using SmartCartPro.Repositories;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services;
using SmartCartPro.Services.Interfaces;
#pragma warning disable CS0108 // Hides inherited member — ICategoryService / IProductService intentional rewrites

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

        // ── Phase 7: Online Orders ────────────────────────────────
        services.AddScoped<IOnlineOrderRepository, OnlineOrderRepository>();
        services.AddScoped<IOnlineOrderService, OnlineOrderService>();

        // ── Phase 7: Reviews ──────────────────────────────────────
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IProductReviewService, ProductReviewService>();

        // ── Phase 7: Cart & Wishlist ──────────────────────────────
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IWishlistService, WishlistService>();

        // ── Phase 7: Settings ─────────────────────────────────────
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISettingService, SettingService>();

        // ── Phase 7: Audit ────────────────────────────────────────
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IAuditService, AuditService>();

        // ── Phase 7: Reports + dashboard ──────────────────────────
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportSuiteRepository, ReportSuiteRepository>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Common filter / dropdown fills (EF LINQ)
        services.AddScoped<IFilterRepository, FilterRepository>();
        services.AddScoped<IFilterService, FilterService>();

        // ── Phase 3: Catalog & Products ───────────────────────────
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

        // ── Phase 4: Inventory ────────────────────────────────────
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IStockService, StockService>();

        // ── Phase 5: Purchasing ───────────────────────────────────
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IPurchaseService, PurchaseService>();

        // ── Phase 6: POS & Billing ────────────────────────────────
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleService, SaleService>();

        return services;
    }

    public static IServiceCollection AddSmartCartAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddDataProtection(); // password-reset tokens (time-limited protector)
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<OtpSettings>(configuration.GetSection(OtpSettings.SectionName));
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
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

        // Permission / module visibility for views (injected as `Access` in _ViewImports)
        services.AddScoped<ICurrentUserAccess, CurrentUserAccess>();

        // Role management
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();

        // Document numbering
        services.AddScoped<IDocumentNumberRepository, DocumentNumberRepository>();
        services.AddScoped<IDocumentNumberService, DocumentNumberService>();

        return services;
    }

    public static IServiceCollection AddSmartCartFilters(this IServiceCollection services)
    {
        // LicenseGateFilter is registered as scoped so it can use ICompanyService.
        // IMemoryCache (AddMemoryCache) must be registered before calling this.
        services.AddScoped<LicenseGateFilter>();
        services.AddScoped<ActiveUserFilter>();
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

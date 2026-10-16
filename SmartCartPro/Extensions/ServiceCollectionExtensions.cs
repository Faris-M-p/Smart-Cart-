using Microsoft.EntityFrameworkCore;
using SmartCartPro.Dapper;
using SmartCartPro.Dapper.Interfaces;
using SmartCartPro.Data;
using SmartCartPro.Repositories;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Extensions;

public static class ServiceCollectionExtensions
{
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

        // Common filter / dropdown fills (Dapper)
        services.AddScoped<IFilterRepository, FilterRepository>();
        services.AddScoped<IFilterService, FilterService>();

        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}

using Ecommerce.Controllers;
using Ecommerce.DataAccess;
using Ecommerce.Interface;
using Ecommerce.Repository;
using Ecommerce.Interface.Admin;
using Ecommerce.Repository.Admin;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Add services to container
// =========================
builder.Services.AddControllersWithViews();

// Register DataAccess
builder.Services.AddScoped<IDataAccessDapper, DataAccessDapper>();

var ecommerceConnection = builder.Configuration.GetConnectionString("Ecommerse")
    ?? throw new InvalidOperationException("Connection string 'Ecommerse' is not configured.");
builder.Services.AddDbContext<EcommerceDbContext>(options =>
    options.UseSqlServer(ecommerceConnection));

// Register custom services
builder.Services.AddCustomServices();

var app = builder.Build();

// =========================
// Configure pipeline
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// =========================
// ROUTING (IMPORTANT PART)
// =========================



// ✅ NORMAL ROUTE (USER)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


// =========================
// Dependency Injection Setup
// =========================
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // IDataAccessDapper is already registered above, no need to register again
        services.AddTransient<ShopInterface, ShopRepository>();
        
        // Admin Services
        services.AddTransient<ICategoryInterface, CategoryRepository>();
        services.AddTransient<ISubCategoryInterface, SubCategoryRepository>();
        services.AddTransient<IProductInterface, ProductRepository>();
        services.AddTransient<IBrandInterface, BrandRepository>();
        services.AddTransient<ISupplierInterface, SupplierRepository>();
        services.AddTransient<IVariantInterface, VariantRepository>();
        services.AddTransient<IVariantValueInterface, VariantValueRepository>();
        services.AddTransient<IProductVariantInterface, ProductVariantRepository>();
        services.AddTransient<IPurchaseInterface, PurchaseRepository>();
        
        return services;
    }
}

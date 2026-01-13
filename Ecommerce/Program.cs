using Ecommerce.Controllers;
using Ecommerce.DataAccess;
using Ecommerce.Interface;
using Ecommerce.Repository;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Add services to container
// =========================
builder.Services.AddControllersWithViews();

// Register DataAccess
builder.Services.AddScoped<DataAccessDapper>();

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
        services.AddScoped<DataAccessDapper>();
        services.AddTransient<ShopInterface, ShopRepository>();
        return services;
    }
}

using Ecommerce.Controllers;
using Ecommerce.DataAccess;
using Ecommerce.Interface;
using Ecommerce.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (optional).
builder.Services.AddControllersWithViews();

// Register DataAccessDapper
builder.Services.AddScoped<DataAccessDapper>();  // Add this line

// Register custom services, including ShopRepository
builder.Services.AddCustomServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Configure default route.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Register DataAccessDapper with Scoped lifetime
        services.AddScoped<DataAccessDapper>();  // Add this line

        services.AddTransient<ShopInterface, ShopRepository>();

        return services;
    }
}

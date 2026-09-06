using System.Text;
using Ecommerce.Configuration;
using Ecommerce.Controllers;
using Ecommerce.DataAccess;
using Ecommerce.Helpers.AdminAuth;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface;
using Ecommerce.Repository;
using Ecommerce.Interface.Admin;
using Ecommerce.Middleware;
using Ecommerce.Repository.Admin;
using Ecommerce.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Add services to container
// =========================
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("CountryStateCity", client =>
{
    client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");
});

// Register DataAccess
builder.Services.AddScoped<IDataAccessDapper, DataAccessDapper>();

var ecommerceConnection = builder.Configuration.GetConnectionString("Ecommerse")
    ?? throw new InvalidOperationException("Connection string 'Ecommerse' is not configured.");
builder.Services.AddDbContext<EcommerceDbContext>(options =>
    options.UseSqlServer(ecommerceConnection));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings is not configured.");
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token)
                    && context.Request.Cookies.TryGetValue(AdminAuthHelper.TokenCookieName, out var cookieToken)
                    && !string.IsNullOrWhiteSpace(cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = AdminAuthHelper.UnauthorizedMessage
                    }));
            }
        };
    });

// Register custom services
builder.Services.AddCustomServices();

var app = builder.Build();

// =========================
// Configure pipeline
// =========================
// Global middleware: all requests and unhandled errors pass through this pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<AdminAuthMiddleware>();
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
        services.AddTransient<IInventoryInterface, InventoryRepository>();
        services.AddTransient<IUserRoleInterface, UserRoleRepository>();
        services.AddTransient<IEmployeeInterface, EmployeeRepository>();
        services.AddTransient<IAdminAuthInterface, AdminAuthRepository>();
        services.AddSingleton<AdminJwtTokenService>();
        services.AddSingleton<IPasswordHasher<AdminUserEntity>, PasswordHasher<AdminUserEntity>>();
        services.AddTransient<IProductVariantImageRepository, ProductVariantImageRepository>();
        services.AddTransient<CommonImageService>();
        
        return services;
    }
}

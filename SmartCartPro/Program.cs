using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using SmartCartPro.Configuration;
using SmartCartPro.Extensions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings is not configured.");
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters.");
}

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
    options.Filters.AddService<LicenseGateFilter>();
});

builder.Services.AddMemoryCache();
builder.Services.AddSmartCartExceptionHandling(builder.Configuration);
builder.Services.AddSmartCartDataAccess(builder.Configuration);
builder.Services.AddSmartCartAuth(builder.Configuration);
builder.Services.AddSmartCartPolicies(); // includes AddAuthorization + PlatformSuperAdmin policy
builder.Services.AddSmartCartFilters();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "username",
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (!string.IsNullOrEmpty(context.Token))
                {
                    return Task.CompletedTask;
                }

                if (context.Request.Cookies.TryGetValue(AuthClaims.TokenCookieName, out var token)
                    && !string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
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

                var returnUrl = context.Request.Path + context.Request.QueryString;
                var loginUrl = "/Auth/Login";
                if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl != "/")
                {
                    loginUrl += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
                }

                if (ExceptionErrorMapper.WantsJsonResponse(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var payload = System.Text.Json.JsonSerializer.Serialize(
                        CommonResponseModel.Failure(
                            "Your session has expired. Please sign in again.",
                            statusCode: 401),
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                        });
                    await context.Response.WriteAsync(payload);
                    return;
                }

                context.Response.Redirect(loginUrl);
            }
        };
    });

var app = builder.Build();

// IExceptionHandler (JSON APIs) first; HTML falls through to /Home/Error page.
// Never use UseDeveloperExceptionPage — friendly Error/404 views instead.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandlingPath = "/Home/Error",
    AllowStatusCode404Response = true
});
app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

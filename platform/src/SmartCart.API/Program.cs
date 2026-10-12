using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartCart.API.Middleware;
using SmartCart.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSmartCartInfrastructure(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? "SmartCart-Platform-Dev-Signing-Key-Change-Me-32+";
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SecretKey must be at least 32 characters.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartCart",
            ValidAudiences = new[]
            {
                builder.Configuration["Jwt:PlatformAudience"] ?? "SmartCartPlatform",
                builder.Configuration["Jwt:CompanyAudience"] ?? "SmartCartCompany",
                builder.Configuration["Jwt:CustomerAudience"] ?? "SmartCartCustomer"
            },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CustomerStorefront", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:CustomerOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5173" })
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CustomerStorefront");
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "SmartCart.API" }));

app.Run();

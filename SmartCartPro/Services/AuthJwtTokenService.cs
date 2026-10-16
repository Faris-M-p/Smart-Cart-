using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartCartPro.Configuration;
using SmartCartPro.Helpers;

namespace SmartCartPro.Services;

public class AuthJwtTokenService
{
    private readonly JwtSettings _settings;

    public AuthJwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(
        int idUser,
        string userName,
        string fullName,
        string email,
        int fkStore,
        int fkCompany,
        string storeCode,
        string storeName,
        bool mustChangePassword,
        bool rememberMe,
        bool isPlatformSuperAdmin = false)
    {
        var minutes = rememberMe
            ? Math.Max(_settings.ExpiryMinutes, 60 * 24 * 14)
            : Math.Max(1, _settings.ExpiryMinutes);

        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, idUser.ToString()),
            new(ClaimTypes.NameIdentifier, idUser.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(AuthClaims.Id_User, idUser.ToString()),
            new(AuthClaims.Fk_Store, fkStore.ToString()),
            new(AuthClaims.Fk_Company, fkCompany.ToString()),
            new(AuthClaims.StoreCode, storeCode),
            new(AuthClaims.StoreName, storeName),
            new(AuthClaims.MustChangePassword, mustChangePassword ? "1" : "0"),
            new(AuthClaims.IsPlatformSuperAdmin, isPlatformSuperAdmin ? "1" : "0"),
            new("username", userName)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

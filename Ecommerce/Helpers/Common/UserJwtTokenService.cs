using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Configuration;
using Ecommerce.Helpers.UserAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Helpers.Common
{
    public class UserJwtTokenService
    {
        private readonly JwtSettings _settings;

        public UserJwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiresAt) CreateToken(int userId, string fullName, string email)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _settings.ExpiryMinutes));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var audience = string.IsNullOrWhiteSpace(_settings.UserAudience)
                ? "SmartCartUser"
                : _settings.UserAudience;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(UserAuthHelper.ClaimUserId, userId.ToString()),
                new(UserAuthHelper.ClaimEmail, email),
                new(UserAuthHelper.ClaimFullName, fullName),
                new(ClaimTypes.Name, fullName),
                new(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}

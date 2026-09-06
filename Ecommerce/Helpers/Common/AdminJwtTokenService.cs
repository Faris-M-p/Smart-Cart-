using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Configuration;
using Ecommerce.Helpers.AdminAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Helpers.Common
{
    public class AdminJwtTokenService
    {
        private readonly JwtSettings _settings;

        public AdminJwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiresAt) CreateToken(
            int employeeId,
            string userName,
            int userRoleId,
            string userRoleName)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _settings.ExpiryMinutes));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, employeeId.ToString()),
                new(ClaimTypes.NameIdentifier, employeeId.ToString()),
                new(ClaimTypes.Name, userName),
                new(AdminAuthHelper.ClaimEmployeeId, employeeId.ToString()),
                new(AdminAuthHelper.ClaimUserName, userName),
                new(AdminAuthHelper.ClaimUserRoleId, userRoleId.ToString()),
                new(AdminAuthHelper.ClaimUserRoleName, userRoleName)
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
}

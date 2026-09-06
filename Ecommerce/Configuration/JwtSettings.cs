namespace Ecommerce.Configuration
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string SecretKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string UserAudience { get; set; } = "SmartCartUser";

        public int ExpiryMinutes { get; set; } = 60;
    }
}

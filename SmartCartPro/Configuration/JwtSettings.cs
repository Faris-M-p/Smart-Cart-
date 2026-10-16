namespace SmartCartPro.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SmartCartPro";
    public string Audience { get; set; } = "SmartCartProAdmin";
    public int ExpiryMinutes { get; set; } = 480;
}

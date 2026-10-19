namespace SmartCartPro.Configuration;

public sealed class SmtpSettings
{
    public const string SectionName = "SmtpSettings";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "SmartCart Pro";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host)
        && !string.IsNullOrWhiteSpace(FromEmail)
        && Port > 0;
}

public sealed class OtpSettings
{
    public const string SectionName = "OtpSettings";

    public int ExpiryMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
    public int Length { get; set; } = 6;

    /// <summary>How long an emailed password-reset link stays valid.</summary>
    public int PasswordResetExpiryMinutes { get; set; } = 30;

    /// <summary>
    /// Optional fixed OTP for local development only. Honoured solely when the host environment is
    /// <c>Development</c>; ignored everywhere else. Leave empty to always use a random emailed code.
    /// </summary>
    public string? DevelopmentFixedCode { get; set; }
}

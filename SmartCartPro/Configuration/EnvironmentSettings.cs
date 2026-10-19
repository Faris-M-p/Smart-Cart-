namespace SmartCartPro.Configuration;

/// <summary>App environment label (Dev / QA / Staging / Prod). Used for error detail visibility.</summary>
public sealed class EnvironmentSettings
{
    public const string SectionName = "EnvironmentSettings";

    /// <summary>Logical environment name, e.g. Dev, QA, Staging, Prod.</summary>
    public string EnvironmentName { get; set; } = "Dev";

    /// <summary>
    /// Public origin used in emailed links (e.g. <c>https://admin.example.com</c>, no trailing slash).
    /// Required outside Development so reset links cannot be poisoned through a forged Host header.
    /// </summary>
    public string? PublicBaseUrl { get; set; }

    public bool IsEnvironment(string name) =>
        string.Equals(EnvironmentName, name, StringComparison.OrdinalIgnoreCase);

    public bool IsProduction => IsEnvironment("Prod") || IsEnvironment("Production");
}

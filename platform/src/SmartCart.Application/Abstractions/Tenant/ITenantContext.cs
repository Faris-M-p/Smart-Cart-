namespace SmartCart.Application.Abstractions.Tenant;

/// <summary>Trusted tenant resolved from authenticated server-side context — never from client-supplied CompanyId.</summary>
public interface ITenantContext
{
    Guid? CompanyAccountId { get; }
    string? CompanyCode { get; }
    bool IsResolved { get; }
}

public sealed class TenantContext : ITenantContext
{
    public Guid? CompanyAccountId { get; set; }
    public string? CompanyCode { get; set; }
    public bool IsResolved => CompanyAccountId.HasValue && CompanyAccountId.Value != Guid.Empty;
}

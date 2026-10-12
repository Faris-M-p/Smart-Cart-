namespace SmartCart.Application.Abstractions.Security;

public static class AuthClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string CompanyCode = "company_code";
    public const string EmployeeId = "employee_id";
    public const string CustomerId = "customer_id";
    public const string PlatformUserId = "platform_user_id";
    public const string ActorType = "actor_type";
}

public static class ActorTypes
{
    public const string SuperAdmin = "super_admin";
    public const string Employee = "employee";
    public const string Customer = "customer";
}

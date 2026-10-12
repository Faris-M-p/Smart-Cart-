namespace SmartCart.Domain.Platform;

/// <summary>Platform-level registry for a client company (not the client-DB Company row).</summary>
public class CompanyAccount
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = CompanyAccountStatuses.Provisioning;
    public string StoreMode { get; set; } = StoreModes.Single;
    public int MaxStoreCount { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }

    public ClientDatabase? Database { get; set; }
    public CompanySubscription? Subscription { get; set; }
    public ICollection<CompanyModule> Modules { get; set; } = new List<CompanyModule>();
    public CompanyTheme? Theme { get; set; }
    public ICollection<CompanySectionAllowance> SectionAllowances { get; set; } = new List<CompanySectionAllowance>();
}

public static class CompanyAccountStatuses
{
    public const string Provisioning = "Provisioning";
    public const string Ready = "Ready";
    public const string Suspended = "Suspended";
    public const string Deactivated = "Deactivated";
    public const string Failed = "Failed";
}

public static class StoreModes
{
    public const string Single = "Single";
    public const string Multi = "Multi";
}

public class ClientDatabase
{
    public Guid Id { get; set; }
    public Guid CompanyAccountId { get; set; }
    public CompanyAccount CompanyAccount { get; set; } = null!;
    /// <summary>Logical database name only — never expose connection secrets to browsers.</summary>
    public string DatabaseName { get; set; } = string.Empty;
    /// <summary>Encrypted connection string (protected via data-protection / KMS in production).</summary>
    public string EncryptedConnectionString { get; set; } = string.Empty;
    public string ProvisioningStatus { get; set; } = "Pending";
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DefaultMaxStores { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class CompanySubscription
{
    public Guid Id { get; set; }
    public Guid CompanyAccountId { get; set; }
    public CompanyAccount CompanyAccount { get; set; } = null!;
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan Plan { get; set; } = null!;
    public DateTimeOffset StartsOn { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
    public string Status { get; set; } = SubscriptionStatuses.Active;
}

public static class SubscriptionStatuses
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Suspended = "Suspended";
}

public class ModuleDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class CompanyModule
{
    public Guid Id { get; set; }
    public Guid CompanyAccountId { get; set; }
    public CompanyAccount CompanyAccount { get; set; } = null!;
    public Guid ModuleDefinitionId { get; set; }
    public ModuleDefinition Module { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
}

public class ThemeDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CssVariablesJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
}

public class CompanyTheme
{
    public Guid Id { get; set; }
    public Guid CompanyAccountId { get; set; }
    public CompanyAccount CompanyAccount { get; set; } = null!;
    public Guid ThemeDefinitionId { get; set; }
    public ThemeDefinition Theme { get; set; } = null!;
}

public class HomepageSectionType
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxItemsDefault { get; set; } = 8;
}

public class CompanySectionAllowance
{
    public Guid Id { get; set; }
    public Guid CompanyAccountId { get; set; }
    public CompanyAccount CompanyAccount { get; set; } = null!;
    public Guid HomepageSectionTypeId { get; set; }
    public HomepageSectionType SectionType { get; set; } = null!;
    public bool IsAllowed { get; set; } = true;
    public int MaxItems { get; set; } = 8;
}

public class PlatformUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class PlatformAuditLog
{
    public long Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? DetailsJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

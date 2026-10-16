namespace SmartCartPro.Entities;

/// <summary>Employee / admin user. Maps to <c>users</c>.</summary>
public class User
{
    public int IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    /// <summary>SQL <c>is_platform_super_admin</c> — platform Super Admin (OTP login). Not company SUPER_ADMIN.</summary>
    public bool IsPlatformSuperAdmin { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public ICollection<UserStoreAccess> StoreAccess { get; set; } = new List<UserStoreAccess>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

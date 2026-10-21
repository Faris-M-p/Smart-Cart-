using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Employee / admin user. Maps to <c>users</c>.</summary>
[Table("users")]
public class User
{
    [Key]
    [Column("id_user")]
    public int IdUser { get; set; }

    [Required]
    [Column("username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("passwordhash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("fullname")]
    public string FullName { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("employeecode")]
    public string? EmployeeCode { get; set; }

    [Column("isactive")]
    public bool IsActive { get; set; } = true;

    [Column("mustchangepassword")]
    public bool MustChangePassword { get; set; } = true;

    /// <summary>SQL <c>is_platform_super_admin</c> — software-team Super Admin (OTP login). Not the company Admin role.</summary>
    [Column("is_platform_super_admin")]
    public bool IsPlatformSuperAdmin { get; set; }

    /// <summary>SQL <c>session_version</c>. Incremented on every new sign-in so older cookies stop working.</summary>
    [Column("session_version")]
    public int SessionVersion { get; set; }

    [Column("lastloginat")]
    public DateTime? LastLoginAt { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }

    public ICollection<UserStoreAccess> StoreAccess { get; set; } = new List<UserStoreAccess>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/admin_users.sql (adminusers).</summary>
    [Table("adminusers")]
    public class AdminUserEntity
    {
        [Key]
        [Column("id_adminuser")]
        public int ID_AdminUser { get; set; }

        [Column("fk_userrole")]
        public int FK_UserRole { get; set; }

        [MaxLength(50)]
        [Column("username")]
        public string UserName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("passwordhash")]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("fullname")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        [Column("phonenumber")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        [Column("profileimageurl")]
        public string? ProfileImageUrl { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}

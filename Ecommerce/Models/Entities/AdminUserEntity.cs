using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[AdminUsers] (see Database/01_Tables/AdminUsers.sql).
    /// </summary>
    [Table("AdminUsers")]
    public class AdminUserEntity
    {
        [Key]
        public int ID_AdminUser { get; set; }

        public int FK_UserRole { get; set; }

        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[Supplier] (see xPROCEDURExTABLES/Tables/Suppliers.sql).
    /// </summary>
    [Table("Supplier")]
    public class SupplierEntity
    {
        [Key]
        public int ID_Supplier { get; set; }

        [MaxLength(250)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? CompanyName { get; set; }

        [MaxLength(250)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string State { get; set; } = string.Empty;

        [MaxLength(150)]
        public string District { get; set; } = string.Empty;

        [MaxLength(150)]
        public string City { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}

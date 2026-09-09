using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/supplier.sql (supplier).</summary>
    [Table("supplier")]
    public class SupplierEntity
    {
        [Key]
        [Column("id_supplier")]
        public int ID_Supplier { get; set; }

        [MaxLength(250)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        [Column("companyname")]
        public string? CompanyName { get; set; }

        [MaxLength(250)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(150)]
        [Column("state")]
        public string State { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("district")]
        public string District { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("city")]
        public string City { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("address")]
        public string? Address { get; set; }

        [MaxLength(10)]
        [Column("pincode")]
        public string? Pincode { get; set; }

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

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

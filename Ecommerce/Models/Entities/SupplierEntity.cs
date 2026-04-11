using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("Suppliers")]
    public class SupplierEntity
    {
        [Key]
        [Column("SupplierId")]
        public int SupplierId { get; set; }

        [MaxLength(100)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(15)]
        public string? ContactPhone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool? Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}

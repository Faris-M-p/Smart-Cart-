using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>Purchase</c> (ID_Purchase, FK_Supplier, PurchaseDate, InvoiceNumber, TotalAmount, Notes, CreatedOn, Cancelled...)</summary>
    [Table("Purchase")]
    public class PurchaseEntity
    {
        [Key]
        [Column("ID_Purchase")]
        public int ID_Purchase { get; set; }

        [Column("FK_Supplier")]
        public int FK_Supplier { get; set; }

        [Column("PurchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [MaxLength(100)]
        public string? InvoiceNumber { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? EnterBy { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        public string? CancelledReason { get; set; }

        public int? CancelledBy { get; set; }
    }
}


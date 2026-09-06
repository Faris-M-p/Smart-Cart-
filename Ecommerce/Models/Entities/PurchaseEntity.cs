using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>Purchase</c> (ID_Purchase, FK_Supplier, PurchaseDate, InvoiceNumber, TotalAmount, Notes, CreatedOn, Cancelled...)</summary>
    [Table("Purchase")]
    public class PurchaseEntity
    {
        [Key]
        public int ID_Purchase { get; set; }

        public int FK_Supplier { get; set; }

        public DateTime PurchaseDate { get; set; }

        [MaxLength(100)]
        public string? InvoiceNumber { get; set; }

        /// <summary>Display-only. The Purchase table has no GRNNumber column.</summary>
        [NotMapped]
        public string? GRNNumber { get; set; }

        /// <summary>Display-only. The Purchase table has no PaymentStatus column.</summary>
        [NotMapped]
        [MaxLength(30)]
        public string? PaymentStatus { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(12,2)")]
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


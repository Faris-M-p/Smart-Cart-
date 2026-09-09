using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/purchase.sql (purchase).</summary>
    [Table("purchase")]
    public class PurchaseEntity
    {
        [Key]
        [Column("id_purchase")]
        public int ID_Purchase { get; set; }

        [Column("fk_supplier")]
        public int FK_Supplier { get; set; }

        [Column("purchasedate")]
        public DateTime PurchaseDate { get; set; }

        [MaxLength(100)]
        [Column("invoicenumber")]
        public string? InvoiceNumber { get; set; }

        /// <summary>Display-only. The Purchase table has no GRNNumber column.</summary>
        [NotMapped]
        public string? GRNNumber { get; set; }

        /// <summary>Display-only. The Purchase table has no PaymentStatus column.</summary>
        [NotMapped]
        [MaxLength(30)]
        public string? PaymentStatus { get; set; }

        [Column("totalamount", TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("enterby")]
        public int? EnterBy { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }

        [Column("cancelledby")]
        public int? CancelledBy { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/sales.sql (sales).</summary>
    [Table("sales")]
    public class SaleEntity
    {
        [Key]
        [Column("id_sale")]
        public int ID_Sale { get; set; }

        [MaxLength(100)]
        [Column("invoicenumber")]
        public string? InvoiceNumber { get; set; }

        [Column("saledate")]
        public DateTime SaleDate { get; set; }

        [MaxLength(200)]
        [Column("customername")]
        public string? CustomerName { get; set; }

        [MaxLength(30)]
        [Column("customerphone")]
        public string? CustomerPhone { get; set; }

        [MaxLength(30)]
        [Column("paymentmethod")]
        public string PaymentMethod { get; set; } = "Cash";

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

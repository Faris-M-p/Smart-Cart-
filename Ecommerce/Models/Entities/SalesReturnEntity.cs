using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/sales_return.sql (salesreturn).</summary>
    [Table("salesreturn")]
    public class SalesReturnEntity
    {
        [Key]
        [Column("id_salesreturn")]
        public int ID_SalesReturn { get; set; }

        [Column("fk_sale")]
        public int FK_Sale { get; set; }

        [MaxLength(100)]
        [Column("invoicenumber")]
        public string? InvoiceNumber { get; set; }

        [Column("returndate")]
        public DateTime ReturnDate { get; set; }

        [Column("totalamount", TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        [Column("reason")]
        public string? Reason { get; set; }

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

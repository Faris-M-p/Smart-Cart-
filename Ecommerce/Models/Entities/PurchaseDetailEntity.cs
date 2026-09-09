using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/purchase_detail.sql (purchasedetail).</summary>
    [Table("purchasedetail")]
    public class PurchaseDetailEntity
    {
        [Key]
        [Column("id_purchasedetail")]
        public int ID_PurchaseDetail { get; set; }

        [Column("fk_purchase")]
        public int FK_Purchase { get; set; }

        [Column("fk_productvariant")]
        public int FK_ProductVariant { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("purchaseprice", TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Column("mrp")]
        public decimal? MRP { get; set; }

        [Column("expirydate")]
        public DateTime? ExpiryDate { get; set; }

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

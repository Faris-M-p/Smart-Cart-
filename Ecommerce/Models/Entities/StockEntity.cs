using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/stock.sql (stock).</summary>
    [Table("stock")]
    public class StockEntity
    {
        [Key]
        [Column("id_stock")]
        public int ID_Stock { get; set; }

        [Column("fk_purchasedetail")]
        public int FK_PurchaseDetail { get; set; }

        [Column("fk_productvariant")]
        public int FK_ProductVariant { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("enterby")]
        public int? EnterBy { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }

        [Column("cancelledby")]
        public int? CancelledBy { get; set; }
    }
}

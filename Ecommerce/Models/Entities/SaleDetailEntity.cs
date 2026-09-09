using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/sales_detail.sql (salesdetail).</summary>
    [Table("salesdetail")]
    public class SaleDetailEntity
    {
        [Key]
        [Column("id_salesdetail")]
        public int ID_SaleDetail { get; set; }

        [Column("fk_sale")]
        public int FK_Sale { get; set; }

        [Column("fk_productvariant")]
        public int FK_ProductVariant { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("sellingprice", TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Column("mrp")]
        public decimal? MRP { get; set; }

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

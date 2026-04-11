using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("Stock")]
    public class StockEntity
    {
        [Key]
        [Column("StockId")]
        public int StockId { get; set; }

        public int? ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MRP { get; set; }

        [Column(TypeName = "decimal(2,1)")]
        public decimal? Rating { get; set; }

        public bool? Cancelled { get; set; }
    }
}

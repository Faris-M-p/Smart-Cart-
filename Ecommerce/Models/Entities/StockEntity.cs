using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("Stock")]
    public class StockEntity
    {
        [Key]
        public int ID_Stock { get; set; }

        public int FK_PurchaseDetail { get; set; }

        public int FK_ProductVariant { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? EnterBy { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }

        public int? CancelledBy { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>PurchaseDetail</c> (ID_PurchaseDetail, FK_Purchase, FK_ProductVariant, Quantity, PurchasePrice, MRP, ExpiryDate, CreatedOn, Cancelled...)</summary>
    [Table("PurchaseDetail")]
    public class PurchaseDetailEntity
    {
        [Key]
        public int ID_PurchaseDetail { get; set; }

        public int FK_Purchase { get; set; }

        public int FK_ProductVariant { get; set; }

        public int Quantity { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        
        public decimal? MRP { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? EnterBy { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        public string? CancelledReason { get; set; }

        public int? CancelledBy { get; set; }
    }
}


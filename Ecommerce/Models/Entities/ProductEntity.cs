using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[Products] (see xPROCEDURExTABLES/Tables/Products.sql).
    /// </summary>
    [Table("Products")]
    public class ProductEntity
    {
        [Key]
        [Column("ProductId")]
        public int ProductId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MRP { get; set; }

        public int? CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public int? BrandId { get; set; }

        [Column(TypeName = "decimal(2,1)")]
        public decimal? Rating { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        public int? StatusId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool? Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}

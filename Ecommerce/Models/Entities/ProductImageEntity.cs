using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[ProductImages] for first-image projection on product list.
    /// </summary>
    [Table("ProductImages")]
    public class ProductImageEntity
    {
        [Key]
        [Column("ProductImageId")]
        public int ProductImageId { get; set; }

        [Column("ProductId")]
        public int ProductId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool? Cancelled { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>Products</c> (ID_Product, FK_SubCategory, FK_Brand, Name, Slug, Description, IsActive, CreatedAt, ModifiedAt, Cancelled, CancelledOn).</summary>
    [Table("Products")]
    public class ProductEntity
    {
        [Key]
        public int ID_Product { get; set; }

        public int FK_SubCategory { get; set; }

        public int? FK_Brand { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool SellOnline { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }
    }
}

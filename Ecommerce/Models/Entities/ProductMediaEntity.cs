using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("ProductMedia")]
    public class ProductMediaEntity
    {
        [Key]
        public int ID_ProductMedia { get; set; }

        public int FK_Product { get; set; }

        [Required]
        [MaxLength(20)]
        public string MediaType { get; set; } = "Image";

        [Required]
        [MaxLength(500)]
        public string MediaUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}

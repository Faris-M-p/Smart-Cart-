using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("Brands")]
    public class BrandEntity
    {
        [Key]
        [Column("BrandId")]
        public int BrandId { get; set; }

        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        public bool? Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}

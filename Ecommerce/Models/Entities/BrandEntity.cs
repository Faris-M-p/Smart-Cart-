using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[Brand] (see xPROCEDURExTABLES/Tables/Brands.sql).
    /// </summary>
    [Table("Brand")]
    public class BrandEntity
    {
        [Key]
        [Column("ID_Brand")]
        public int BrandId { get; set; }

        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}

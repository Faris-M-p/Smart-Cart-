using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/subcategory.sql (subcategory).</summary>
    [Table("subcategory")]
    public class SubCategoryEntity
    {
        [Key]
        [Column("id_subcategory")]
        public int ID_SubCategory { get; set; }

        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("fk_category")]
        public int FK_Category { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        [Column("imageurl")]
        public string? ImageUrl { get; set; }

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}

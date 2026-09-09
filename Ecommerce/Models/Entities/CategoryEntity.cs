using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/category.sql (category).</summary>
    [Table("category")]
    public class CategoryEntity
    {
        [Key]
        [Column("id_category")]
        public int ID_Category { get; set; }

        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(1000)]
        [Column("imageurl")]
        public string? ImageUrl { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}

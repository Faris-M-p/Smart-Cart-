using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[SubCategory] (see ProSubCategoryUpdate / ProSubCategoryListSelect).
    /// </summary>
    [Table("SubCategory")]
    public class SubCategoryEntity
    {
        [Key]
        [Column("ID_SubCategory")]
        public int IdSubCategory { get; set; }

        [MaxLength(255)]
        public string SubCategoryName { get; set; } = string.Empty;

        [Column("FK_Category")]
        public int FkCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        public string? CancelledReason { get; set; }
    }
}

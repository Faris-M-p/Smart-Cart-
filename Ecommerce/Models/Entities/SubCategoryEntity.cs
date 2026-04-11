using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Minimal mapping for category delete checks (see ProCategoryDelete).
    /// </summary>
    [Table("SubCategory")]
    public class SubCategoryEntity
    {
        [Key]
        [Column("ID_SubCategory")]
        public int IdSubCategory { get; set; }

        [Column("FK_Category")]
        public int FkCategory { get; set; }

        public bool? Cancelled { get; set; }
    }
}

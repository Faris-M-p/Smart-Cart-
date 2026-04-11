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
       
        public int ID_SubCategory { get; set; }

        /// <summary>Maps to database column <c>Name</c> (not SubCategoryName).</summary>
        
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

      
        public int FK_Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool? Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        public string? CancelledReason { get; set; }
    }
}

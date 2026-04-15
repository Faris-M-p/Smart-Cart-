using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>ProductVariantAttributes</c> (ID_ProductVariantAttribute, FK_ProductVariant, FK_Variant, FK_VariantValue).</summary>
    [Table("ProductVariantAttributes")]
    public class ProductVariantAttributeEntity
    {
        [Key]
        public int ID_ProductVariantAttribute { get; set; }

        public int FK_ProductVariant { get; set; }

        public int FK_Variant { get; set; }

        public int FK_VariantValue { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}

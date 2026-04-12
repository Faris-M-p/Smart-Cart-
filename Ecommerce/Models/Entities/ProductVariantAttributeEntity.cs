using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>ProductVariantAttributes</c> (ID_ProductVariantAttribute, FK_ProductVariant, FK_Variant, FK_VariantValue).</summary>
    [Table("ProductVariantAttributes")]
    public class ProductVariantAttributeEntity
    {
        [Key]
        [Column("ID_ProductVariantAttribute")]
        public int IdProductVariantAttribute { get; set; }

        [Column("FK_ProductVariant")]
        public int FkProductVariant { get; set; }

        [Column("FK_Variant")]
        public int FkVariant { get; set; }

        [Column("FK_VariantValue")]
        public int FkVariantValue { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}

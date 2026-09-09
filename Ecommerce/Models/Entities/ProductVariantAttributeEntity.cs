using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/product_variant_attributes.sql (productvariantattributes).</summary>
    [Table("productvariantattributes")]
    public class ProductVariantAttributeEntity
    {
        [Key]
        [Column("id_productvariantattribute")]
        public int ID_ProductVariantAttribute { get; set; }

        [Column("fk_productvariant")]
        public int FK_ProductVariant { get; set; }

        [Column("fk_variant")]
        public int FK_Variant { get; set; }

        [Column("fk_variantvalue")]
        public int FK_VariantValue { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }
    }
}

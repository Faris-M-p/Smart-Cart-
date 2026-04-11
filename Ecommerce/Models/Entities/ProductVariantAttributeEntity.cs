using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("ProductVariantAttribute")]
    public class ProductVariantAttributeEntity
    {
        [Column("FK_ProductVariant")]
        public int FkProductVariant { get; set; }

        [Column("FK_Variant")]
        public int FkVariant { get; set; }

        [Column("FK_VariantValue")]
        public int FkVariantValue { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("homepage_banners")]
    public class HomepageBannerEntity
    {
        [Key]
        [Column("id_banner")]
        public int ID_Banner { get; set; }

        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [Column("imageurl")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    [Table("homepage_banner_categories")]
    public class HomepageBannerCategoryEntity
    {
        [Key]
        [Column("id_bannercategory")]
        public int ID_BannerCategory { get; set; }

        [Column("fk_banner")]
        public int FK_Banner { get; set; }

        [Column("fk_category")]
        public int FK_Category { get; set; }
    }

    [Table("homepage_categories")]
    public class HomepageCategoryEntity
    {
        [Key]
        [Column("id_homepagecategory")]
        public int ID_HomepageCategory { get; set; }

        [Column("fk_category")]
        public int FK_Category { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    [Table("homepage_products")]
    public class HomepageProductEntity
    {
        [Key]
        [Column("id_homepageproduct")]
        public int ID_HomepageProduct { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}

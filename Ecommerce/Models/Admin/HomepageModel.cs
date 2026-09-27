using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Models.Admin
{
    public class HomepageModel
    {
        // -------------------------------------------------------------
        // Banners
        // -------------------------------------------------------------
        public class BannerDto
        {
            public int BannerId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Subtitle { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public string? TargetUrl { get; set; }
            public bool IsActive { get; set; }
            public int DisplayOrder { get; set; }
            public List<int> CategoryIds { get; set; } = new();
            public List<string> CategoryNames { get; set; } = new();
            public DateTime CreatedAt { get; set; }
        }

        public class BannerSaveInputVIEW
        {
            public int BannerId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Subtitle { get; set; }
            public string? TargetUrl { get; set; }
            public IFormFile? BannerImage { get; set; }
            public bool RemoveImage { get; set; }
            public bool? IsActive { get; set; }
            public int DisplayOrder { get; set; }
            public List<int> CategoryIds { get; set; } = new();
        }

        public class BannerSaveInput
        {
            public int BannerId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Subtitle { get; set; }
            public string? TargetUrl { get; set; }
            public string? ImageUrl { get; set; }
            public bool IsActive { get; set; } = true;
            public int DisplayOrder { get; set; }
            public List<int> CategoryIds { get; set; } = new();
        }

        // -------------------------------------------------------------
        // Featured Categories
        // -------------------------------------------------------------
        public class FeaturedCategoryDto
        {
            public int HomepageCategoryId { get; set; }
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public int DisplayOrder { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class FeaturedCategorySaveInputVIEW
        {
            public int HomepageCategoryId { get; set; }

            [Required(ErrorMessage = "Please select a category.")]
            public int CategoryId { get; set; }
            public bool? IsActive { get; set; }
            public int DisplayOrder { get; set; }
        }

        // -------------------------------------------------------------
        // Featured Products
        // -------------------------------------------------------------
        public class FeaturedProductDto
        {
            public int HomepageProductId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsActive { get; set; }
            public int DisplayOrder { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class FeaturedProductSaveInputVIEW
        {
            public int HomepageProductId { get; set; }

            [Required(ErrorMessage = "Please select a product.")]
            public int ProductId { get; set; }
            public bool? IsActive { get; set; }
            public int DisplayOrder { get; set; }
        }

        // -------------------------------------------------------------
        // Lookups
        // -------------------------------------------------------------
        public class CategoryOption
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
        }

        public class ProductOption
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class ProductModel
    {
        public class InputProduct
        {
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public string SearchName { get; set; } = "";

            [Range(0, 4, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            public string SortMode { get; set; } = "ASC";
            public string CategoryIds { get; set; } = "";
            public string SubCategoryIds { get; set; } = "";
            public string BrandIds { get; set; } = "";
            public string Ratings { get; set; } = "";
            public string Gender { get; set; } = "";
            public decimal? PriceFrom { get; set; }
            public decimal? PriceTo { get; set; }
            public string Status { get; set; } = "";
        }

        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int SubCategoryId { get; set; }
            public int BrandId { get; set; }
            public string BrandName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public int Rating { get; set; }
            public string Gender { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal MRP { get; set; }
            public bool InStock { get; set; }
        }

        public class ShopFilterOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? CategoryId { get; set; }
        }

        public class ShopFilterLookups
        {
            public List<ShopFilterOption> Categories { get; set; } = new();
            public List<ShopFilterOption> SubCategories { get; set; } = new();
            public List<ShopFilterOption> Brands { get; set; } = new();
        }

        public class ShopProductDetails
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int SubCategoryId { get; set; }
            public string SubCategoryName { get; set; } = string.Empty;
            public int BrandId { get; set; }
            public string BrandName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public List<string> ImageUrls { get; set; } = new();
            public decimal Price { get; set; }
            public decimal MRP { get; set; }
            public bool InStock { get; set; }
        }
    }
}

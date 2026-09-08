using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ecommerce.CustomModelValidation;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Models.Admin
{
    public class ProductModel
    {
        public class ProductListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter SubCategory IDs")]
            public string FilterSubCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter Brand IDs")]
            public string FilterBrandIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            [Range(0, 3, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class ProductUpdateInputVIEW
        {
            [Display(Name = "Product ID")]
            public int ID_Product { get; set; }

            [Display(Name = "Category")]
            [GreaterThanZero(ErrorMessage = "{0} must be selected.")]
            public int FK_Category { get; set; }

            [Display(Name = "SubCategory")]
            [GreaterThanZero(ErrorMessage = "{0} must be selected.")]
            public int FK_SubCategory { get; set; }

            [Display(Name = "Brand")]
            public int? FK_Brand { get; set; }

            [Display(Name = "Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Slug")]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Slug { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Sell Online")]
            public bool SellOnline { get; set; }

            public List<IFormFile> Files { get; set; } = new();
            public List<int> ExistingMediaOrder { get; set; } = new();
            public List<int> RemovedMediaIds { get; set; } = new();
            public int? PrimaryIndex { get; set; }
            public int? PrimaryMediaId { get; set; }
        }

        public class ProductMediaDeleteInput
        {
            [GreaterThanZero]
            public int MediaId { get; set; }
        }

        public class ProductMediaSetPrimaryInput
        {
            [GreaterThanZero]
            public int ProductId { get; set; }

            [GreaterThanZero]
            public int MediaId { get; set; }

            public List<int> OrderedMediaIds { get; set; } = new();
        }

        public class ProductDeleteInputVIEW
        {
            [Display(Name = "Product ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int ID_Product { get; set; }
        }

        public class ProductListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string FilterCategoryIDs { get; set; } = string.Empty;
            public string FilterSubCategoryIDs { get; set; } = string.Empty;
            public string FilterBrandIDs { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = string.Empty;
        }

        public class ProductUpdateInput
        {
            public int ID_Product { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int FK_SubCategory { get; set; }
            public int? FK_Brand { get; set; }
            public bool IsActive { get; set; } = true;
            public bool SellOnline { get; set; }
            public int EnterBy { get; set; } = 1;
        }

        public class ProductDeleteInput
        {
            public int ID_Product { get; set; }
            public int EnterBy { get; set; } = 1;
        }

        /// <summary>List/detail row for admin product screens.</summary>
        public class Product
        {
            [JsonPropertyName("productId")]
            public int ID_Product { get; set; }

            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;

            [JsonPropertyName("fkSubCategory")]
            public int FK_SubCategory { get; set; }

            [JsonPropertyName("fkBrand")]
            public int? FK_Brand { get; set; }

            [JsonPropertyName("fkCategory")]
            public int FK_Category { get; set; }

            public string? CategoryName { get; set; }
            public string? SubCategoryName { get; set; }
            public string? BrandName { get; set; }
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public bool IsActive { get; set; }
            public bool SellOnline { get; set; }
            public bool Cancelled { get; set; }
            public List<ProductMediaDto> Media { get; set; } = new();
        }

        public class ProductMediaDto
        {
            [JsonPropertyName("idProductMedia")]
            public int ID_ProductMedia { get; set; }

            [JsonPropertyName("fkProduct")]
            public int FK_Product { get; set; }

            [JsonPropertyName("mediaType")]
            public string MediaType { get; set; } = "Image";

            [JsonPropertyName("mediaUrl")]
            public string MediaUrl { get; set; } = string.Empty;

            [JsonPropertyName("isPrimary")]
            public bool IsPrimary { get; set; }

            [JsonPropertyName("displayOrder")]
            public int DisplayOrder { get; set; }
        }
    }
}

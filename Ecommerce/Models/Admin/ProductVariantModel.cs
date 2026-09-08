using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ecommerce.CustomModelValidation;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Models.Admin
{
    public class ProductVariantModel
    {
        public class ProductVariantListInputVIEW
        {
            [Display(Name = "Product ID")]
            [GreaterThanZero]
            public int FK_Product { get; set; }

            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Variant IDs")]
            public string FilterVariantIDs { get; set; } = string.Empty;

            [Display(Name = "Filter Variant Value IDs")]
            public string FilterVariantValueIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 5000, ErrorMessage = "{0} must be between 1 and 5000.")]
            public int PageSize { get; set; } = 20;

            [Display(Name = "Sort Column")]
            [Range(0, 4, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = "ASC";
        }

        /// <summary>UI row: one variant type + chosen value (JSON: variantId, variantValueId).</summary>
        public class VariantValueRowVIEW
        {
            [GreaterThanZero]
            [JsonPropertyName("variantId")]
            public int VariantId { get; set; }

            [GreaterThanZero]
            [JsonPropertyName("variantValueId")]
            public int VariantValueId { get; set; }
        }

        public class ProductVariantUpdateInputVIEW
        {
            public int ID_ProductVariant { get; set; }

            [GreaterThanZero]
            public int FK_Product { get; set; }

            [Required(ErrorMessage = "SKU is required.")]
            [RequiredNotEmpty]
            [MaxLength(100)]
            public string SKU { get; set; } = string.Empty;

            [MaxLength(255)]
            public string VariantLabel { get; set; } = string.Empty;

            [Range(typeof(decimal), "0", "79228162514264337593543950335")]
            public decimal MRP { get; set; }

            [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Selling price must be greater than zero.")]
            public decimal SellingPrice { get; set; }

            public bool IsActive { get; set; } = true;

            public bool IsDefault { get; set; }

            public List<VariantValueRowVIEW> VariantValues { get; set; } = new();

            // Used when request is sent as multipart/form-data.
            public string VariantValuesJson { get; set; } = string.Empty;

            // Image fields submitted in the same create/update request.
            public List<IFormFile> Files { get; set; } = new();
            public List<int> ExistingImageOrder { get; set; } = new();
            public List<int> RemovedImageIds { get; set; } = new();
            public int? PrimaryIndex { get; set; }
            public int? PrimaryImageId { get; set; }
        }

        public class ProductVariantDeleteInputVIEW
        {
            [GreaterThanZero]
            public int ID_ProductVariant { get; set; }

            [MaxLength(500)]
            public string? CancelledReason { get; set; }
        }

        public class ProductVariantImageUploadInput
        {
            [GreaterThanZero]
            public int SKUId { get; set; }

            public List<IFormFile> Files { get; set; } = new();

            public int? PrimaryIndex { get; set; }

            public List<int> ExistingImageOrder { get; set; } = new();
        }

        public class ProductVariantImageDeleteInput
        {
            [GreaterThanZero]
            public int ImageId { get; set; }
        }

        public class ProductVariantImageSetPrimaryInput
        {
            [GreaterThanZero]
            public int SKUId { get; set; }

            [GreaterThanZero]
            public int ImageId { get; set; }

            public List<int> OrderedImageIds { get; set; } = new();
        }

        public class ProductVariantListInput
        {
            public int FK_Product { get; set; }
            public string SearchText { get; set; } = string.Empty;
            public string FilterVariantIDs { get; set; } = string.Empty;
            public string FilterVariantValueIDs { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = "ASC";
        }

        public class ProductVariantValueRowInput
        {
            public int VariantId { get; set; }
            public int VariantValueId { get; set; }
        }

        public class ProductVariantUpdateInput
        {
            public int ID_ProductVariant { get; set; }
            public int FK_Product { get; set; }
            public string SKU { get; set; } = string.Empty;
            public string VariantLabel { get; set; } = string.Empty;
            public decimal MRP { get; set; }
            public decimal SellingPrice { get; set; }
            public bool IsActive { get; set; } = true;
            public bool IsDefault { get; set; }
            public List<ProductVariantValueRowInput> VariantValues { get; set; } = new();
            public int EnterBy { get; set; } = 1;
        }

        public class ProductVariantDeleteInput
        {
            public int ID_ProductVariant { get; set; }
            public int EnterBy { get; set; } = 1;
        }

        public class ProductVariant
        {
            [JsonPropertyName("idProductVariant")]
            public int ID_ProductVariant { get; set; }

            [JsonPropertyName("fkProduct")]
            public int FK_Product { get; set; }

            [JsonPropertyName("sku")]
            public string SKU { get; set; } = string.Empty;

            [JsonPropertyName("variantLabel")]
            public string VariantLabel { get; set; } = string.Empty;

            [JsonPropertyName("combination")]
            public string Combination { get; set; } = string.Empty;

            [JsonPropertyName("attributeSignature")]
            public string AttributeSignature { get; set; } = string.Empty;

            [JsonPropertyName("mrp")]
            public decimal MRP { get; set; }

            [JsonPropertyName("sellingPrice")]
            public decimal SellingPrice { get; set; }

            [JsonPropertyName("price")]
            public decimal Price { get; set; }

            [JsonPropertyName("productName")]
            public string ProductName { get; set; } = string.Empty;

            [JsonPropertyName("isActive")]
            public bool IsActive { get; set; }

            [JsonPropertyName("isDefault")]
            public bool IsDefault { get; set; }

            [JsonPropertyName("cancelled")]
            public bool Cancelled { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime? CreatedAt { get; set; }

            [JsonPropertyName("totalImages")]
            public int TotalImages { get; set; }

            [JsonPropertyName("imageUrl")]
            public string? ImageUrl { get; set; }
        }

        public class ProductVariantDetail
        {
            [JsonPropertyName("idProductVariant")]
            public int ID_ProductVariant { get; set; }

            [JsonPropertyName("fkProduct")]
            public int FK_Product { get; set; }

            [JsonPropertyName("sku")]
            public string SKU { get; set; } = string.Empty;

            [JsonPropertyName("variantLabel")]
            public string VariantLabel { get; set; } = string.Empty;

            [JsonPropertyName("mrp")]
            public decimal MRP { get; set; }

            [JsonPropertyName("sellingPrice")]
            public decimal SellingPrice { get; set; }

            [JsonPropertyName("isActive")]
            public bool IsActive { get; set; }

            [JsonPropertyName("isDefault")]
            public bool IsDefault { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime? CreatedAt { get; set; }

            [JsonPropertyName("attributes")]
            public List<VariantAttributeDetail> Attributes { get; set; } = new();

            [JsonPropertyName("variantValues")]
            public List<VariantValueRowDetail> VariantValues { get; set; } = new();

            [JsonPropertyName("images")]
            public List<ProductVariantImageDto> Images { get; set; } = new();

            [JsonPropertyName("primaryImageId")]
            public int? PrimaryImageId { get; set; }

            [JsonPropertyName("imageOrder")]
            public List<int> ImageOrder { get; set; } = new();
        }

        public class ProductVariantImageDto
        {
            [JsonPropertyName("idProductVariantImage")]
            public int ID_ProductVariantImage { get; set; }

            [JsonPropertyName("fkProductVariant")]
            public int FK_ProductVariant { get; set; }

            [JsonPropertyName("imageUrl")]
            public string ImageUrl { get; set; } = string.Empty;

            [JsonPropertyName("isPrimary")]
            public bool IsPrimary { get; set; }

            [JsonPropertyName("displayOrder")]
            public int DisplayOrder { get; set; }
        }

        public class VariantValueRowDetail
        {
            [JsonPropertyName("variantId")]
            public int VariantId { get; set; }

            [JsonPropertyName("variantValueId")]
            public int VariantValueId { get; set; }
        }

        public class VariantAttributeDetail
        {
            [JsonPropertyName("fkVariant")]
            public int FK_Variant { get; set; }

            [JsonPropertyName("variantName")]
            public string VariantName { get; set; } = string.Empty;

            [JsonPropertyName("fkVariantValue")]
            public int FK_VariantValue { get; set; }

            [JsonPropertyName("valueName")]
            public string ValueName { get; set; } = string.Empty;
        }
    }
}

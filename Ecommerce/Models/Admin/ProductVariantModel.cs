using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ecommerce.CustomModelValidation;

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

            [MinLength(1, ErrorMessage = "At least one variant value is required.")]
            public List<VariantValueRowVIEW> VariantValues { get; set; } = new();
        }

        public class ProductVariantDeleteInputVIEW
        {
            [GreaterThanZero]
            public int ID_ProductVariant { get; set; }

            [MaxLength(500)]
            public string? CancelledReason { get; set; }
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

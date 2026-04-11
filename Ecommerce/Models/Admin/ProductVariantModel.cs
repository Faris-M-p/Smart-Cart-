using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class ProductVariantModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class ProductVariantListInputVIEW
        {
            [Display(Name = "Product ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int FK_Product { get; set; }

            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Variant IDs")]
            public string FilterVariantIDs { get; set; } = string.Empty; // JSON array

            [Display(Name = "Filter Variant Value IDs")]
            public string FilterVariantValueIDs { get; set; } = string.Empty; // JSON array

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 20;

            [Display(Name = "Sort Column")]
            [Range(0, 3, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = "ASC"; // ASC / DESC
        }

        public class ProductVariantUpdateInputVIEW
        {
            [Display(Name = "Product Variant ID")]
            public int ID_ProductVariant { get; set; } = 0;

            [Display(Name = "Product")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int FK_Product { get; set; }

            [Display(Name = "Price Adjustment")]
            public decimal PriceAdjustment { get; set; } = 0;

            [Display(Name = "Is Default")]
            public bool IsDefault { get; set; } = false;

            [Display(Name = "Variant Attributes")]
            [Required(ErrorMessage = "{0} is required.")]
            public List<VariantAttributeVIEW> VariantAttributes { get; set; } = new List<VariantAttributeVIEW>();
        }

        public class VariantAttributeVIEW
        {
            [Display(Name = "Variant")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int FK_Variant { get; set; }

            [Display(Name = "Variant Value")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int FK_VariantValue { get; set; }
        }

        public class ProductVariantDeleteInputVIEW
        {
            [Display(Name = "Product Variant ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int ID_ProductVariant { get; set; }

            [Display(Name = "Cancelled Reason")]
            [MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Stored Procedures
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

        public class ProductVariantUpdateInput
        {
            public int UserAction { get; set; } // 1=Insert, 2=Update, 3=Delete
            public int ID_ProductVariant { get; set; } = 0;
            public int FK_Product { get; set; } = 0;
            public decimal PriceAdjustment { get; set; } = 0;
            public bool IsDefault { get; set; } = false;
            public string VariantAttributes { get; set; } = string.Empty; // JSON string
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
            public string? CancelledReason { get; set; }
        }

        public class ProductVariantDeleteInput
        {
            public int UserAction { get; set; } = 3; // Always 3 for delete
            public int ID_ProductVariant { get; set; }
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
            public string? CancelledReason { get; set; }
        }

        // Output Models from Stored Procedures
        public class ProductVariant
        {
            public int ID_ProductVariant { get; set; }
            public int FK_Product { get; set; }
            public decimal PriceAdjustment { get; set; }
            public bool IsDefault { get; set; }
            public DateTime CreatedOn { get; set; }
            public string AttributeSignature { get; set; } = string.Empty; // e.g., "Color:Black | Print:Spiderman"
            public int StockAvailable { get; set; }
            public string? ImageURL { get; set; }
        }

        public class ProductVariantDetail
        {
            public int ID_ProductVariant { get; set; }
            public int FK_Product { get; set; }
            public decimal PriceAdjustment { get; set; }
            public bool IsDefault { get; set; }
            public DateTime CreatedOn { get; set; }
            public List<VariantAttributeDetail> Attributes { get; set; } = new List<VariantAttributeDetail>();
        }

        public class VariantAttributeDetail
        {
            public int FK_Variant { get; set; }
            public string VariantName { get; set; } = string.Empty;
            public int FK_VariantValue { get; set; }
            public string ValueName { get; set; } = string.Empty;
        }
    }
}

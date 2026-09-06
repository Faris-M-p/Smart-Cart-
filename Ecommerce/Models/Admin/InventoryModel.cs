using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class InventoryModel
    {
        public class StockListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Sort by Low Stock")]
            public bool LowStockOnly { get; set; } = false;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 20;
        }

        public class StockListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public bool LowStockOnly { get; set; } = false;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 20;
        }

        public class StockRow
        {
            public int FK_ProductVariant { get; set; }
            public string SKU { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public int AvailableQty { get; set; }
            public int ReservedQty { get; set; }
            public int ReorderLevel { get; set; }
            public DateTime? LastUpdated { get; set; }
        }

        public class AdjustStockInputVIEW
        {
            [Display(Name = "Product Variant")]
            [GreaterThanZero]
            public int FK_ProductVariant { get; set; }

            [Display(Name = "Adjust By")]
            public int AdjustBy { get; set; }

            [Display(Name = "Reason")]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Reason { get; set; } = string.Empty;

            public int EnterBy { get; set; } = 1;
        }

        public class AdjustStockInput
        {
            public int FK_ProductVariant { get; set; }
            public int AdjustBy { get; set; }
            public string Reason { get; set; } = string.Empty;
            public int EnterBy { get; set; } = 1;
        }
    }
}


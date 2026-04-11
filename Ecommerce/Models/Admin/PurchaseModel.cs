using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class PurchaseModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class PurchaseListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Supplier IDs")]
            public string FilterSupplierIDs { get; set; } = string.Empty; // JSON array

            [Display(Name = "From Date")]
            public DateTime? FromDate { get; set; }

            [Display(Name = "To Date")]
            public DateTime? ToDate { get; set; }

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
            public string SortMode { get; set; } = "DESC"; // ASC / DESC
        }

        public class PurchaseUpdateInputVIEW
        {
            [Display(Name = "Purchase ID")]
            public int ID_Purchase { get; set; } = 0;

            [Display(Name = "Supplier")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int FK_Supplier { get; set; }

            [Display(Name = "Purchase Date")]
            [Required(ErrorMessage = "{0} is required.")]
            public DateTime PurchaseDate { get; set; } = DateTime.Now;

            [Display(Name = "Invoice Number")]
            [MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string InvoiceNumber { get; set; } = string.Empty;

            [Display(Name = "Notes")]
            [MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string Notes { get; set; } = string.Empty;

            [Display(Name = "Purchase Details")]
            [Required(ErrorMessage = "{0} is required.")]
            public List<PurchaseDetailVIEW> PurchaseDetails { get; set; } = new List<PurchaseDetailVIEW>();
        }

        public class PurchaseDetailVIEW
        {
            [Display(Name = "Purchase Detail ID")]
            public int ID_PurchaseDetail { get; set; } = 0; // 0 for new, existing ID for update

            [Display(Name = "Product Variant (SKU)")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int FK_ProductVariant { get; set; }

            [Display(Name = "Quantity")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int Quantity { get; set; }

            [Display(Name = "Purchase Price")]
            [Required(ErrorMessage = "{0} is required.")]
            [Range(0, double.MaxValue, ErrorMessage = "{0} must be greater than or equal to 0.")]
            public decimal PurchasePrice { get; set; }

            [Display(Name = "MRP")]
            [Range(0, double.MaxValue, ErrorMessage = "{0} must be greater than or equal to 0.")]
            public decimal? MRP { get; set; }

            [Display(Name = "Expiry Date")]
            public DateTime? ExpiryDate { get; set; }
        }

        public class PurchaseDeleteInputVIEW
        {
            [Display(Name = "Purchase ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int ID_Purchase { get; set; }

            [Display(Name = "Cancelled Reason")]
            [MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Stored Procedures
        public class PurchaseListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string FilterSupplierIDs { get; set; } = string.Empty;
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = "DESC";
        }

        public class PurchaseUpdateInput
        {
            public int UserAction { get; set; } // 1=Insert, 2=Update, 3=Delete
            public int ID_Purchase { get; set; } = 0;
            public int FK_Supplier { get; set; } = 0;
            public DateTime? PurchaseDate { get; set; }
            public string InvoiceNumber { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public string PurchaseDetails { get; set; } = string.Empty; // JSON string
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
            public string? CancelledReason { get; set; }
        }

        public class PurchaseDeleteInput
        {
            public int UserAction { get; set; } = 3; // Always 3 for delete
            public int ID_Purchase { get; set; }
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
            public string? CancelledReason { get; set; }
        }

        // Output Models from Stored Procedures
        public class Purchase
        {
            public int ID_Purchase { get; set; }
            public int FK_Supplier { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public string InvoiceNumber { get; set; } = string.Empty;
            public DateTime PurchaseDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string? CancelledReason { get; set; }
        }

        public class PurchaseDetail
        {
            public int ID_PurchaseDetail { get; set; }
            public int FK_Purchase { get; set; }
            public int FK_ProductVariant { get; set; }
            public int FK_Product { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string VariantAttributes { get; set; } = string.Empty; // e.g., "Color:Black | Size:XL"
            public int Quantity { get; set; }
            public decimal PurchasePrice { get; set; }
            public decimal? MRP { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
        }

        public class Stock
        {
            public int ID_Stock { get; set; }
            public int FK_PurchaseDetail { get; set; }
            public int FK_ProductVariant { get; set; }
            public int Quantity { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string? CancelledReason { get; set; }
        }

        public class PurchaseDetailFull
        {
            public Purchase PurchaseHeader { get; set; } = new Purchase();
            public List<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();
            public List<Stock> StockBatches { get; set; } = new List<Stock>();
        }
    }
}

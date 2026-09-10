using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class SaleModel
    {
        public class SaleListInputVIEW
        {
            public string SearchText { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }

            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [GreaterThanZero]
            [Range(1, 100)]
            public int PageSize { get; set; } = 10;

            [Range(0, 3)]
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = "DESC";
        }

        public class SaleDetailVIEW
        {
            public int ID_SaleDetail { get; set; }

            [Required]
            [GreaterThanZero]
            public int FK_ProductVariant { get; set; }

            [Required]
            [GreaterThanZero]
            public int Quantity { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            public decimal SellingPrice { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? MRP { get; set; }
        }

        public class SaleUpdateInputVIEW
        {
            public int ID_Sale { get; set; }

            [Required]
            public DateTime SaleDate { get; set; } = DateTime.Today;

            [MaxLength(200)]
            public string CustomerName { get; set; } = string.Empty;

            [MaxLength(30)]
            public string CustomerPhone { get; set; } = string.Empty;

            [Required]
            [MaxLength(30)]
            public string PaymentMethod { get; set; } = "Cash";

            [MaxLength(100)]
            public string InvoiceNumber { get; set; } = string.Empty;

            [MaxLength(500)]
            public string Notes { get; set; } = string.Empty;

            [Required]
            public List<SaleDetailVIEW> SaleDetails { get; set; } = new();
        }

        public class SaleDeleteInputVIEW
        {
            [Required]
            [GreaterThanZero]
            public int ID_Sale { get; set; }

            [MaxLength(500)]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class SaleListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = "DESC";
        }

        public class SaleUpdateInput
        {
            public int ID_Sale { get; set; }
            public DateTime SaleDate { get; set; } = DateTime.Today;
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerPhone { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = "Cash";
            public string InvoiceNumber { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public int EnterBy { get; set; } = 1;
            public List<SaleDetailVIEW> SaleDetails { get; set; } = new();
        }

        public class SaleDeleteInput
        {
            public int ID_Sale { get; set; }
            public int EnterBy { get; set; } = 1;
            public string? CancelledReason { get; set; }
        }

        public class Sale
        {
            public int ID_Sale { get; set; }
            public string InvoiceNumber { get; set; } = string.Empty;
            public DateTime SaleDate { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerPhone { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string? CancelledReason { get; set; }
            public bool HasReturns { get; set; }
        }

        public class SaleDetail
        {
            public int ID_SaleDetail { get; set; }
            public int FK_Sale { get; set; }
            public int FK_ProductVariant { get; set; }
            public int FK_Product { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string VariantLabel { get; set; } = string.Empty;
            public string SKU { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal? MRP { get; set; }
            public decimal LineTotal { get; set; }
            public int ReturnedQty { get; set; }
            public int ReturnableQty { get; set; }
        }

        public class SaleDetailFull
        {
            public Sale SaleHeader { get; set; } = new();
            public List<SaleDetail> SaleDetails { get; set; } = new();
        }

        public class SaleSkuOption
        {
            public int FK_Product { get; set; }
            public int ID_ProductVariant { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string VariantLabel { get; set; } = string.Empty;
            public string SKU { get; set; } = string.Empty;
            public string Barcode { get; set; } = string.Empty;
            public decimal SellingPrice { get; set; }
            public decimal MRP { get; set; }
            public int AvailableQty { get; set; }
        }

        public class PosCustomerOption
        {
            public int? UserId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Source { get; set; } = "sale";
        }

        public class SaleAdjacentResult
        {
            public Sale? Sale { get; set; }
            public bool HasPrevious { get; set; }
            public bool HasNext { get; set; }
        }
    }
}

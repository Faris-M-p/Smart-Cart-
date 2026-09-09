using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class SalesReturnModel
    {
        public class SalesReturnListInputVIEW
        {
            public string SearchText { get; set; } = string.Empty;
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

        public class SalesReturnDetailVIEW
        {
            [Required]
            [GreaterThanZero]
            public int FK_SaleDetail { get; set; }

            [Required]
            [GreaterThanZero]
            public int Quantity { get; set; }
        }

        public class SalesReturnUpdateInputVIEW
        {
            public int ID_SalesReturn { get; set; }

            [Required]
            [GreaterThanZero]
            public int FK_Sale { get; set; }

            [Required]
            public DateTime ReturnDate { get; set; } = DateTime.Today;

            [MaxLength(500)]
            public string Reason { get; set; } = string.Empty;

            [MaxLength(500)]
            public string Notes { get; set; } = string.Empty;

            [Required]
            public List<SalesReturnDetailVIEW> ReturnDetails { get; set; } = new();
        }

        public class SalesReturnDeleteInputVIEW
        {
            [Required]
            [GreaterThanZero]
            public int ID_SalesReturn { get; set; }

            [MaxLength(500)]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class SalesReturnListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int SortColumn { get; set; }
            public string SortMode { get; set; } = "DESC";
        }

        public class SalesReturnUpdateInput
        {
            public int FK_Sale { get; set; }
            public DateTime ReturnDate { get; set; } = DateTime.Today;
            public string Reason { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public int EnterBy { get; set; } = 1;
            public List<SalesReturnDetailVIEW> ReturnDetails { get; set; } = new();
        }

        public class SalesReturnDeleteInput
        {
            public int ID_SalesReturn { get; set; }
            public int EnterBy { get; set; } = 1;
            public string? CancelledReason { get; set; }
        }

        public class SalesReturn
        {
            public int ID_SalesReturn { get; set; }
            public int FK_Sale { get; set; }
            public string InvoiceNumber { get; set; } = string.Empty;
            public string SaleInvoiceNumber { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public DateTime ReturnDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string? Reason { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string? CancelledReason { get; set; }
        }

        public class SalesReturnDetail
        {
            public int ID_SalesReturnDetail { get; set; }
            public int FK_SalesReturn { get; set; }
            public int FK_SaleDetail { get; set; }
            public int FK_ProductVariant { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string VariantLabel { get; set; } = string.Empty;
            public string SKU { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal LineTotal { get; set; }
        }

        public class SalesReturnDetailFull
        {
            public SalesReturn ReturnHeader { get; set; } = new();
            public List<SalesReturnDetail> ReturnDetails { get; set; } = new();
        }

        public class SaleLookup
        {
            public int ID_Sale { get; set; }
            public string InvoiceNumber { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public DateTime SaleDate { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}

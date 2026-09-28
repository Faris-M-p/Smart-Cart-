using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.Admin
{
    public class ReportModel
    {
        public class SalesReportInput
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }

        public class SalesReportSummary
        {
            public int TotalOrders { get; set; }
            public decimal TotalSales { get; set; }
            public List<MethodSummary> PaymentMethodSummary { get; set; } = new();
            public List<StatusSummary> OrderStatusSummary { get; set; } = new();
            public List<DailySummary> DailySales { get; set; } = new();
        }

        public class MethodSummary { public string Method { get; set; } public int Count { get; set; } public decimal Total { get; set; } }
        public class StatusSummary { public string Status { get; set; } public int Count { get; set; } public decimal Total { get; set; } }
        public class DailySummary { public string Date { get; set; } public int Count { get; set; } public decimal Total { get; set; } }

        public class OrderReportInput
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string OrderStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string PaymentMethod { get; set; }
            public string SearchText { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        public class OrderReportItem
        {
            public string OrderNumber { get; set; }
            public string CustomerName { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentStatus { get; set; }
            public string OrderStatus { get; set; }
        }

        public class ProductReportInput
        {
            public string SearchText { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        public class ProductReportItem
        {
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public int QuantitySold { get; set; }
            public decimal SalesAmount { get; set; }
            public int CurrentStock { get; set; }
        }

        public class CustomerReportInput
        {
            public string SearchText { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        public class CustomerReportItem
        {
            public string CustomerName { get; set; }
            public string Email { get; set; }
            public int TotalOrders { get; set; }
            public decimal TotalPurchaseAmount { get; set; }
            public DateTime? LastOrderDate { get; set; }
        }
    }
}

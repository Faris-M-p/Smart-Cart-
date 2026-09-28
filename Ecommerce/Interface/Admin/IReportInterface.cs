using static Ecommerce.Models.Admin.ReportModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface.Admin
{
    public interface IReportInterface
    {
        Task<SalesReportSummary> GetSalesReportAsync(SalesReportInput input);
        Task<TableOutput<OrderReportItem>> GetOrderReportAsync(OrderReportInput input);
        Task<TableOutput<ProductReportItem>> GetProductReportAsync(ProductReportInput input);
        Task<TableOutput<CustomerReportItem>> GetCustomerReportAsync(CustomerReportInput input);
    }
}

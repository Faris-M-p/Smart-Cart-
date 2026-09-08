using static Ecommerce.Models.Admin.AdminOrderModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface.Admin
{
    public interface IAdminOrderInterface
    {
        Task<TableOutput<AdminOrderListItem>> GetOrderListAsync(AdminOrderListInput input);
        Task<AdminOrderDetail?> GetOrderByIdAsync(int orderId);
        Task<CommonResponse> ConfirmOrderAsync(int orderId);
        Task<CommonResponse> UpdateOrderStatusAsync(int orderId, string orderStatus);
        Task<CommonResponse> DeliverOrderAsync(int orderId);
        Task<CommonResponse> CancelOrderAsync(int orderId, string reason);
    }
}

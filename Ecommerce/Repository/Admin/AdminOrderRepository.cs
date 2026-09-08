using System.Data;
using Dapper;
using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.AdminOrderModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class AdminOrderRepository : IAdminOrderInterface
    {
        private readonly IDataAccessDapper _dapper;

        public AdminOrderRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public Task<TableOutput<AdminOrderListItem>> GetOrderListAsync(AdminOrderListInput input)
        {
            input ??= new AdminOrderListInput();

            return _dapper.GetMultipleListByStoredProcedure<AdminOrderListItem>("GetAdminOrders", new
            {
                SearchText = (input.SearchText ?? string.Empty).Trim(),
                OrderStatus = (input.OrderStatus ?? string.Empty).Trim(),
                FromDate = input.FromDate,
                ToDate = input.ToDate,
                PageIndex = input.PageIndex < 1 ? 1 : input.PageIndex,
                PageSize = input.PageSize < 1 ? 10 : input.PageSize
            });
        }

        public async Task<AdminOrderDetail?> GetOrderByIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }

            using var connection = _dapper.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "GetAdminOrder",
                new { OrderId = orderId },
                commandType: CommandType.StoredProcedure);

            var header = await multi.ReadFirstOrDefaultAsync<AdminOrderHeader>();
            if (header == null)
            {
                return null;
            }

            return new AdminOrderDetail
            {
                Order = header,
                Items = (await multi.ReadAsync<AdminOrderItem>()).ToList()
            };
        }

        public Task<CommonResponse> ConfirmOrderAsync(int orderId)
        {
            return _dapper.ExecuteStoredProcedure("AdminConfirmOrder", new { OrderId = orderId });
        }

        public Task<CommonResponse> UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            return _dapper.ExecuteStoredProcedure("AdminUpdateOrderStatus", new
            {
                OrderId = orderId,
                OrderStatus = (orderStatus ?? string.Empty).Trim()
            });
        }

        public Task<CommonResponse> DeliverOrderAsync(int orderId)
        {
            return _dapper.ExecuteStoredProcedure("AdminDeliverOrder", new { OrderId = orderId });
        }

        public Task<CommonResponse> CancelOrderAsync(int orderId, string reason)
        {
            return _dapper.ExecuteStoredProcedure("AdminCancelOrder", new
            {
                OrderId = orderId,
                Reason = (reason ?? string.Empty).Trim()
            });
        }
    }
}

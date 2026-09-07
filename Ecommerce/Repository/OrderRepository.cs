using System.Data;
using Dapper;
using Ecommerce.Interface;
using static Ecommerce.Models.CartModel;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;

namespace Ecommerce.Repository
{
    public class OrderRepository : OrderInterface
    {
        private readonly IDataAccessDapper _dapper;

        public OrderRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<CheckoutPage> GetPreviewAsync(int userId, int productVariantId, int quantity)
        {
            using var connection = _dapper.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "GetCheckoutPreview",
                new
                {
                    UserId = userId,
                    ProductVariantId = productVariantId,
                    Quantity = quantity < 1 ? 1 : quantity
                },
                commandType: CommandType.StoredProcedure);

            return new CheckoutPage
            {
                Source = productVariantId > 0 ? "buynow" : "cart",
                Items = (await multi.ReadAsync<CartLine>()).ToList(),
                Summary = await multi.ReadFirstOrDefaultAsync<CheckoutSummary>() ?? new CheckoutSummary(),
                Customer = await multi.ReadFirstOrDefaultAsync<CheckoutCustomer>() ?? new CheckoutCustomer()
            };
        }

        public Task<CommonResponse> PlaceOrderAsync(int userId, PlaceOrderInput input)
        {
            return _dapper.ExecuteStoredProcedure("PlaceOrder", new
            {
                UserId = userId,
                ProductVariantId = input.ProductVariantId,
                Quantity = input.Quantity < 1 ? 1 : input.Quantity,
                ReceiverName = (input.ReceiverName ?? string.Empty).Trim(),
                Phone = (input.Phone ?? string.Empty).Trim(),
                AddressLine = (input.AddressLine ?? string.Empty).Trim(),
                City = (input.City ?? string.Empty).Trim(),
                Pincode = (input.Pincode ?? string.Empty).Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(input.PaymentMethod) ? "COD" : input.PaymentMethod.Trim()
            });
        }

        public async Task<OrderPage?> GetOrderAsync(int userId, int orderId)
        {
            using var connection = _dapper.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "GetOrder",
                new { UserId = userId, OrderId = orderId },
                commandType: CommandType.StoredProcedure);

            var header = await multi.ReadFirstOrDefaultAsync<OrderHeader>();
            if (header == null)
            {
                return null;
            }

            return new OrderPage
            {
                Order = header,
                Items = (await multi.ReadAsync<CartLine>()).ToList()
            };
        }

        public async Task<List<OrderListItem>> GetOrdersAsync(int userId)
        {
            using var connection = _dapper.CreateConnection();
            var orders = await connection.QueryAsync<OrderListItem>(
                "GetOrders",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return orders.ToList();
        }

        public Task<CommonResponse> CancelOrderAsync(int userId, CancelOrderInput input)
        {
            return _dapper.ExecuteStoredProcedure("CancelOrder", new
            {
                UserId = userId,
                OrderId = input.OrderId,
                Reason = (input.Reason ?? string.Empty).Trim()
            });
        }
    }
}

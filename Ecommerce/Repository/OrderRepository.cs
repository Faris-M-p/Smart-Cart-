using Ecommerce.Interface;
using Ecommerce.Models;
using static Ecommerce.Models.CartModel;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;
using static Ecommerce.Models.UserAddressModel;

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
            var multi = await _dapper.GetMultipleListsByProcedure<CartLine, CheckoutSummary, CheckoutCustomer, object>(
                StoredProcedures.Order.GetCheckoutPreview,
                new
                {
                    UserId = userId,
                    ProductVariantId = productVariantId,
                    Quantity = quantity < 1 ? 1 : quantity
                },
                new[] { "p_result", "p_result2", "p_result3" });

            List<UserAddress> addresses;
            try
            {
                addresses = await GetUserAddressesAsync(userId) ?? new List<UserAddress>();
            }
            catch
            {
                addresses = new List<UserAddress>();
            }

            return new CheckoutPage
            {
                Source = productVariantId > 0 ? "buynow" : "cart",
                Items = multi.TableOut1 ?? new List<CartLine>(),
                Summary = multi.TableOut2?.FirstOrDefault() ?? new CheckoutSummary(),
                Customer = multi.TableOut3?.FirstOrDefault() ?? new CheckoutCustomer(),
                Addresses = addresses
            };
        }

        public async Task<CommonResponse> PlaceOrderAsync(int userId, PlaceOrderInput input)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Order.PlaceOrder,
                new
                {
                    UserId = userId,
                    ProductVariantId = input.ProductVariantId,
                    Quantity = input.Quantity < 1 ? 1 : input.Quantity,
                    ReceiverName = (input.ReceiverName ?? string.Empty).Trim(),
                    Phone = (input.Phone ?? string.Empty).Trim(),
                    AddressLine = (input.AddressLine ?? string.Empty).Trim(),
                    City = (input.City ?? string.Empty).Trim(),
                    Pincode = (input.Pincode ?? string.Empty).Trim(),
                    PaymentMethod = string.IsNullOrWhiteSpace(input.PaymentMethod) ? "COD" : input.PaymentMethod.Trim(),
                    RazorpayOrderId = input.RazorpayOrderId,
                    RazorpayPaymentId = input.RazorpayPaymentId
                }) ?? StatusFail();
        }

        public async Task<OrderPage?> GetOrderAsync(int userId, int orderId)
        {
            var multi = await _dapper.GetMultipleListsByProcedure<OrderHeader, CartLine, object>(
                StoredProcedures.Order.GetOrder,
                new { UserId = userId, OrderId = orderId },
                new[] { "p_result", "p_result2" });

            var header = multi.TableOut1?.FirstOrDefault();
            if (header == null)
            {
                return null;
            }

            return new OrderPage
            {
                Order = header,
                Items = multi.TableOut2 ?? new List<CartLine>()
            };
        }

        public async Task<List<OrderListItem>> GetOrdersAsync(int userId)
        {
            return await _dapper.GetListByProcedure<OrderListItem, object>(
                StoredProcedures.Order.GetOrders,
                new { UserId = userId });
        }

        public async Task<CommonResponse> CancelOrderAsync(int userId, CancelOrderInput input)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Order.CancelOrder,
                new
                {
                    UserId = userId,
                    OrderId = input.OrderId,
                    Reason = (input.Reason ?? string.Empty).Trim()
                }) ?? StatusFail();
        }

        public async Task<List<UserAddress>> GetUserAddressesAsync(int userId)
        {
            return await _dapper.GetListByProcedure<UserAddress, object>(
                StoredProcedures.Auth.GetUserAddresses,
                new { UserId = userId });
        }

        public async Task<CommonResponse> SaveUserAddressAsync(int userId, SaveAddressInput input)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Auth.SaveUserAddress,
                new
                {
                    UserId = userId,
                    AddressId = input.AddressId,
                    AddressType = (input.AddressType ?? "Home").Trim(),
                    ReceiverName = (input.ReceiverName ?? string.Empty).Trim(),
                    Phone = (input.Phone ?? string.Empty).Trim(),
                    AddressLine = (input.AddressLine ?? string.Empty).Trim(),
                    City = (input.City ?? string.Empty).Trim(),
                    Pincode = (input.Pincode ?? string.Empty).Trim(),
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    IsDefault = input.IsDefault
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> DeleteUserAddressAsync(int userId, int addressId)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Auth.DeleteUserAddress,
                new
                {
                    UserId = userId,
                    AddressId = addressId
                }) ?? StatusFail();
        }

        private static CommonResponse StatusFail() => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = "No response from stored procedure."
        };
    }
}

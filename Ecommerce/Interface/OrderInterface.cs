using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;
using static Ecommerce.Models.UserAddressModel;

namespace Ecommerce.Interface
{
    public interface OrderInterface
    {
        Task<CheckoutPage> GetPreviewAsync(int userId, int productVariantId, int quantity);

        Task<CommonResponse> PlaceOrderAsync(int userId, PlaceOrderInput input);

        Task<OrderPage?> GetOrderAsync(int userId, int orderId);

        Task<List<OrderListItem>> GetOrdersAsync(int userId);

        Task<CommonResponse> CancelOrderAsync(int userId, CancelOrderInput input);

        Task<List<UserAddress>> GetUserAddressesAsync(int userId);

        Task<CommonResponse> SaveUserAddressAsync(int userId, SaveAddressInput input);

        Task<CommonResponse> DeleteUserAddressAsync(int userId, int addressId);
    }
}

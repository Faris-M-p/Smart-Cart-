using static Ecommerce.Models.CartModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface
{
    public interface CartInterface
    {
        Task<BagCounts> GetCountsAsync(int userId);

        Task<CartPage> GetCartAsync(int userId);

        Task<CommonResponse> AddItemAsync(int userId, CartItemInput input);

        Task<CommonResponse> UpdateItemAsync(int userId, CartItemUpdateInput input);

        Task<CommonResponse> RemoveItemAsync(int userId, int cartItemId);

        Task<CommonResponse> ClearAsync(int userId);
    }
}

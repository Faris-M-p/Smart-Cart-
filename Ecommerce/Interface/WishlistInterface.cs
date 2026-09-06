using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.WishlistModel;

namespace Ecommerce.Interface
{
    public interface WishlistInterface
    {
        Task<List<WishlistLine>> GetWishlistAsync(int userId);

        Task<WishlistStatus> GetStatusAsync(int userId, int productId);

        Task<CommonResponse> ToggleAsync(int userId, int productId);

        Task<CommonResponse> RemoveItemAsync(int userId, int wishlistItemId);
    }
}

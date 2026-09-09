using Ecommerce.Interface;
using Ecommerce.Models;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.WishlistModel;

namespace Ecommerce.Repository
{
    public class WishlistRepository : WishlistInterface
    {
        private readonly IDataAccessDapper _dapper;

        public WishlistRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<List<WishlistLine>> GetWishlistAsync(int userId)
        {
            return await _dapper.GetListByProcedure<WishlistLine, object>(
                StoredProcedures.Wishlist.GetWishlist,
                new { UserId = userId });
        }

        public async Task<WishlistStatus> GetStatusAsync(int userId, int productId)
        {
            var status = await _dapper.GetSingleByProcedure<WishlistStatus, object>(
                StoredProcedures.Wishlist.GetWishlistStatus,
                new { UserId = userId, ProductId = productId });

            return status ?? new WishlistStatus();
        }

        public async Task<CommonResponse> ToggleAsync(int userId, int productId)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Wishlist.ToggleWishlist,
                new
                {
                    UserId = userId,
                    ProductId = productId
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> RemoveItemAsync(int userId, int wishlistItemId)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Wishlist.RemoveWishlistItem,
                new
                {
                    UserId = userId,
                    WishlistItemId = wishlistItemId
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

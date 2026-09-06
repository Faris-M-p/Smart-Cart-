using System.Data;
using Dapper;
using Ecommerce.Interface;
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
            using var connection = _dapper.CreateConnection();
            var items = await connection.QueryAsync<WishlistLine>(
                "GetWishlist",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }

        public async Task<WishlistStatus> GetStatusAsync(int userId, int productId)
        {
            using var connection = _dapper.CreateConnection();
            var status = await connection.QueryFirstOrDefaultAsync<WishlistStatus>(
                "GetWishlistStatus",
                new { UserId = userId, ProductId = productId },
                commandType: CommandType.StoredProcedure);

            return status ?? new WishlistStatus();
        }

        public Task<CommonResponse> ToggleAsync(int userId, int productId)
        {
            return _dapper.ExecuteStoredProcedure("ToggleWishlist", new
            {
                UserId = userId,
                ProductId = productId
            });
        }

        public Task<CommonResponse> RemoveItemAsync(int userId, int wishlistItemId)
        {
            return _dapper.ExecuteStoredProcedure("RemoveWishlistItem", new
            {
                UserId = userId,
                WishlistItemId = wishlistItemId
            });
        }
    }
}

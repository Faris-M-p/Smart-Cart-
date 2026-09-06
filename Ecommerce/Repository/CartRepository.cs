using System.Data;
using Dapper;
using Ecommerce.Interface;
using static Ecommerce.Models.CartModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository
{
    public class CartRepository : CartInterface
    {
        private readonly IDataAccessDapper _dapper;

        public CartRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<BagCounts> GetCountsAsync(int userId)
        {
            using var connection = _dapper.CreateConnection();
            var counts = await connection.QueryFirstOrDefaultAsync<BagCounts>(
                "GetBagCounts",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);

            return counts ?? new BagCounts();
        }

        public async Task<CartPage> GetCartAsync(int userId)
        {
            using var connection = _dapper.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "GetCart",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);

            return new CartPage
            {
                Items = (await multi.ReadAsync<CartLine>()).ToList(),
                Summary = await multi.ReadFirstOrDefaultAsync<CartSummary>() ?? new CartSummary()
            };
        }

        public Task<CommonResponse> AddItemAsync(int userId, CartItemInput input)
        {
            return _dapper.ExecuteStoredProcedure("AddCartItem", new
            {
                UserId = userId,
                input.ProductVariantId,
                Quantity = input.Quantity < 1 ? 1 : input.Quantity
            });
        }

        public Task<CommonResponse> UpdateItemAsync(int userId, CartItemUpdateInput input)
        {
            return _dapper.ExecuteStoredProcedure("UpdateCartItem", new
            {
                UserId = userId,
                input.CartItemId,
                input.Quantity
            });
        }

        public Task<CommonResponse> RemoveItemAsync(int userId, int cartItemId)
        {
            return _dapper.ExecuteStoredProcedure("RemoveCartItem", new
            {
                UserId = userId,
                CartItemId = cartItemId
            });
        }

        public Task<CommonResponse> ClearAsync(int userId)
        {
            return _dapper.ExecuteStoredProcedure("ClearCart", new { UserId = userId });
        }
    }
}

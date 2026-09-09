using Ecommerce.Interface;
using Ecommerce.Models;
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
            var counts = await _dapper.GetSingleByProcedure<BagCounts, object>(
                StoredProcedures.Cart.GetBagCounts,
                new { UserId = userId });

            return counts ?? new BagCounts();
        }

        public async Task<CartPage> GetCartAsync(int userId)
        {
            var multi = await _dapper.GetMultipleListsByProcedure<CartLine, CartSummary, object>(
                StoredProcedures.Cart.GetCart,
                new { UserId = userId },
                new[] { "p_result", "p_result2" });

            return new CartPage
            {
                Items = multi.TableOut1 ?? new List<CartLine>(),
                Summary = multi.TableOut2?.FirstOrDefault() ?? new CartSummary()
            };
        }

        public async Task<CommonResponse> AddItemAsync(int userId, CartItemInput input)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Cart.AddCartItem,
                new
                {
                    UserId = userId,
                    input.ProductVariantId,
                    Quantity = input.Quantity < 1 ? 1 : input.Quantity
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> UpdateItemAsync(int userId, CartItemUpdateInput input)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Cart.UpdateCartItem,
                new
                {
                    UserId = userId,
                    input.CartItemId,
                    input.Quantity
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> RemoveItemAsync(int userId, int cartItemId)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Cart.RemoveCartItem,
                new
                {
                    UserId = userId,
                    CartItemId = cartItemId
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> ClearAsync(int userId)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Cart.ClearCart,
                new { UserId = userId }) ?? StatusFail();
        }

        private static CommonResponse StatusFail() => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = "No response from stored procedure."
        };
    }
}

using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductModel;
using Dapper;

namespace Ecommerce.Repository.Admin
{
    public class ProductRepository : IProductInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public ProductRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<Product>> GetProductListAsync(ProductListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<Product, ProductListInput>(
                    storedProcedureName: "ProProductListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching product list.", ex);
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    var product = await connection.QueryFirstOrDefaultAsync<Product>(
                        "ProProductSelectById",
                        new { ID_Product = id },
                        commandType: System.Data.CommandType.StoredProcedure
                    );
                    return product;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching product by ID.", ex);
            }
        }

        public async Task<CommonResponse> CreateProductAsync(ProductUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductUpdateInput>(
                    storedProcedureName: "ProProductUpdate",
                    parameter: input
                );
                return response;
            }
            catch (Exception ex)
            {
                return new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred while creating product: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdateProductAsync(ProductUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductUpdateInput>(
                    storedProcedureName: "ProProductUpdate",
                    parameter: input
                );
                return response;
            }
            catch (Exception ex)
            {
                return new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred while updating product: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeleteProductAsync(ProductDeleteInput input)
        {
            try
            {
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductDeleteInput>(
                    storedProcedureName: "ProProductDelete",
                    parameter: input
                );
                return response;
            }
            catch (Exception ex)
            {
                return new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred while deleting product: {ex.Message}"
                };
            }
        }
    }
}

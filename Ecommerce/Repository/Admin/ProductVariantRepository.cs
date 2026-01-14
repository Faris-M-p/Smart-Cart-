using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductVariantModel;
using Dapper;
using System.Data;
using Ecommerce.Interface;

namespace Ecommerce.Repository.Admin
{
    public class ProductVariantRepository : IProductVariantInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public ProductVariantRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<ProductVariant>> GetProductVariantListAsync(ProductVariantListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<ProductVariant, ProductVariantListInput>(
                    storedProcedureName: "ProProductVariantSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching product variant list.", ex);
            }
        }

        public async Task<ProductVariantDetail> GetProductVariantByIdAsync(int id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    // First get the base ProductVariant info
                    var variant = await connection.QueryFirstOrDefaultAsync<ProductVariant>(
                        "SELECT ID_ProductVariant, FK_Product, PriceAdjustment, IsDefault, CreatedOn FROM ProductVariant WHERE ID_ProductVariant = @ID_ProductVariant AND Cancelled = 0",
                        new { ID_ProductVariant = id }
                    );

                    if (variant == null)
                        return null;

                    // Get the attributes
                    var attributes = await connection.QueryAsync<VariantAttributeDetail>(
                        @"SELECT pva.FK_Variant, v.VariantName, pva.FK_VariantValue, vv.ValueName
                          FROM ProductVariantAttribute pva
                          INNER JOIN Variant v ON pva.FK_Variant = v.ID_Variant
                          INNER JOIN VariantValue vv ON pva.FK_VariantValue = vv.ID_VariantValue
                          WHERE pva.FK_ProductVariant = @ID_ProductVariant",
                        new { ID_ProductVariant = id }
                    );

                    return new ProductVariantDetail
                    {
                        ID_ProductVariant = variant.ID_ProductVariant,
                        FK_Product = variant.FK_Product,
                        PriceAdjustment = variant.PriceAdjustment,
                        IsDefault = variant.IsDefault,
                        CreatedOn = variant.CreatedOn,
                        Attributes = attributes.ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching product variant by ID.", ex);
            }
        }

        public async Task<CommonResponse> CreateProductVariantAsync(ProductVariantUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductVariantUpdateInput>(
                    storedProcedureName: "ProProductVariantUpsert",
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
                    ResponseMsg = $"An error occurred while creating product variant: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdateProductVariantAsync(ProductVariantUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductVariantUpdateInput>(
                    storedProcedureName: "ProProductVariantUpsert",
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
                    ResponseMsg = $"An error occurred while updating product variant: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeleteProductVariantAsync(ProductVariantDeleteInput input)
        {
            try
            {
                input.UserAction = 3; // 3 = Delete
                // For delete, we need to use ProProductVariantUpsert with UserAction = 3
                var updateInput = new ProductVariantUpdateInput
                {
                    UserAction = 3,
                    ID_ProductVariant = input.ID_ProductVariant,
                    FK_Product = 0, // Will be fetched in procedure
                    PriceAdjustment = 0,
                    IsDefault = false,
                    VariantAttributes = null, // Not needed for delete
                    EnterBy = input.EnterBy,
                    CancelledReason = input.CancelledReason
                };
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductVariantUpdateInput>(
                    storedProcedureName: "ProProductVariantUpsert",
                    parameter: updateInput
                );
                return response;
            }
            catch (Exception ex)
            {
                return new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred while deleting product variant: {ex.Message}"
                };
            }
        }
    }
}

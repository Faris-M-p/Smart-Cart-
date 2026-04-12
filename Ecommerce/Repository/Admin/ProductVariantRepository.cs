using Ecommerce.DataAccess;
using Ecommerce.Helpers.ProductVariants;
using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductVariantModel;

namespace Ecommerce.Repository.Admin
{
    public class ProductVariantRepository : IProductVariantInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;
        private readonly EcommerceDbContext _dbContext;

        public ProductVariantRepository(IDataAccessDapper dataAccessDapper, EcommerceDbContext dbContext)
        {
            _dataAccessDapper = dataAccessDapper;
            _dbContext = dbContext;
        }

        public async Task<TableOutput<ProductVariant>> GetProductVariantListAsync(ProductVariantListInput input)
        {
            if (input == null)
            {
                return ProductVariantHelper.EmptyTableOutput(null);
            }

            try
            {
                var spParams = ProductVariantHelper.BuildProductVariantListSpParameters(input);
                return await _dataAccessDapper.GetMultipleListByStoredProcedure<ProductVariant>(
                    storedProcedureName: "ProProductVariantSelect",
                    parameter: spParams);
            }
            catch
            {
                return ProductVariantHelper.EmptyTableOutput(input);
            }
        }

        public async Task<ProductVariantDetail?> GetProductVariantByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {
                var row = await _dbContext.ProductVariants.AsNoTracking()
                    .FirstOrDefaultAsync(pv => pv.IdProductVariant == id && pv.Cancelled != true);

                if (row == null)
                {
                    return null;
                }

                var attributes = await (
                    from pva in _dbContext.ProductVariantAttributes.AsNoTracking()
                    join v in _dbContext.Variants.AsNoTracking() on pva.FkVariant equals v.IdVariant
                    join vv in _dbContext.VariantValues.AsNoTracking() on pva.FkVariantValue equals vv.IdVariantValue
                    where pva.FkProductVariant == id
                    select new VariantAttributeDetail
                    {
                        FK_Variant = pva.FkVariant,
                        VariantName = v.Name,
                        FK_VariantValue = pva.FkVariantValue,
                        ValueName = vv.Name
                    }).ToListAsync();

                return new ProductVariantDetail
                {
                    ID_ProductVariant = row.IdProductVariant,
                    FK_Product = row.FkProduct,
                    PriceAdjustment = row.PriceAdjustment,
                    IsDefault = row.IsDefault,
                    CreatedOn = row.CreatedOn,
                    Attributes = attributes
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<CommonResponse> CreateProductVariantAsync(ProductVariantUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductVariantUpdateInput>(
                    storedProcedureName: "ProProductVariantUpsert",
                    parameter: input);
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
                    parameter: input);
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
                var updateInput = new ProductVariantUpdateInput
                {
                    UserAction = 3,
                    ID_ProductVariant = input.ID_ProductVariant,
                    FK_Product = 0,
                    PriceAdjustment = 0,
                    IsDefault = false,
                    VariantAttributes = string.Empty,
                    EnterBy = input.EnterBy,
                    CancelledReason = input.CancelledReason
                };
                var response = await _dataAccessDapper.ExecuteStoredProcedure<ProductVariantUpdateInput>(
                    storedProcedureName: "ProProductVariantUpsert",
                    parameter: updateInput);
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

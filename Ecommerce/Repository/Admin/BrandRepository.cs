using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.BrandModel;
using Dapper;
using Ecommerce.Interface;

namespace Ecommerce.Repository.Admin
{
    public class BrandRepository : IBrandInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public BrandRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<Brand>> GetBrandListAsync(BrandListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<Brand, BrandListInput>(
                    storedProcedureName: "ProBrandListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching brand list.", ex);
            }
        }

        public async Task<Brand> GetBrandByIdAsync(int id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    var brand = await connection.QueryFirstOrDefaultAsync<Brand>(
                        "ProBrandSelectById",
                        new { BrandID = id },
                        commandType: System.Data.CommandType.StoredProcedure
                    );
                    return brand;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching brand by ID.", ex);
            }
        }

        public async Task<CommonResponse> CreateBrandAsync(BrandUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<BrandUpdateInput>(
                    storedProcedureName: "ProBrandUpdate",
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
                    ResponseMsg = $"An error occurred while creating brand: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdateBrandAsync(BrandUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<BrandUpdateInput>(
                    storedProcedureName: "ProBrandUpdate",
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
                    ResponseMsg = $"An error occurred while updating brand: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeleteBrandAsync(BrandDeleteInput input)
        {
            try
            {
                var response = await _dataAccessDapper.ExecuteStoredProcedure<BrandDeleteInput>(
                    storedProcedureName: "ProBrandDelete",
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
                    ResponseMsg = $"An error occurred while deleting brand: {ex.Message}"
                };
            }
        }
    }
}

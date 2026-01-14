using Dapper;
using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantModel;
using System.Data;

namespace Ecommerce.Repository.Admin
{
    public class VariantRepository : IVariantInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public VariantRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<VariantListOutput>> GetVariantListAsync(VariantListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<VariantListOutput, VariantListInput>(
                    storedProcedureName: "ProVariantListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching variant list.", ex);
            }
        }

        public async Task<CommonResponse> UpdateVariantAsync(VariantUpdateInput input)
        {
            try
            {
                var response = await _dataAccessDapper.ExecuteStoredProcedure<VariantUpdateInput>(
                    storedProcedureName: "ProVariantUpdate",
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
                    ResponseMsg = $"An error occurred while updating variant: {ex.Message}"
                };
            }
        }

        public async Task<VariantSelectByIdOutput?> GetVariantByIdAsync(int id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    var variant = await connection.QueryFirstOrDefaultAsync<VariantSelectByIdOutput>(
                        "ProVariantSelectById",
                        new { ID_Variant = id },
                        commandType: CommandType.StoredProcedure
                    );
                    return variant;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching variant by ID.", ex);
            }
        }
    }
}

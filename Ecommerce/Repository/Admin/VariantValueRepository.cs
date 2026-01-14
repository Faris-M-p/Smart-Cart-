using Dapper;
using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantValueModel;
using System.Data;

namespace Ecommerce.Repository.Admin
{
    public class VariantValueRepository : IVariantValueInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public VariantValueRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<VariantValueListOutput>> GetVariantValueListAsync(VariantValueListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<VariantValueListOutput, VariantValueListInput>(
                    storedProcedureName: "ProVariantValueListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching variant value list.", ex);
            }
        }

        public async Task<CommonResponse> UpdateVariantValueAsync(VariantValueUpdateInput input)
        {
            try
            {
                var response = await _dataAccessDapper.ExecuteStoredProcedure<VariantValueUpdateInput>(
                    storedProcedureName: "ProVariantValueUpdate",
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
                    ResponseMsg = $"An error occurred while updating variant value: {ex.Message}"
                };
            }
        }

        public async Task<VariantValueSelectByIdOutput?> GetVariantValueByIdAsync(int id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    var variantValue = await connection.QueryFirstOrDefaultAsync<VariantValueSelectByIdOutput>(
                        "ProVariantValueSelectById",
                        new { ID_VariantValue = id },
                        commandType: CommandType.StoredProcedure
                    );
                    return variantValue;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching variant value by ID.", ex);
            }
        }
    }
}

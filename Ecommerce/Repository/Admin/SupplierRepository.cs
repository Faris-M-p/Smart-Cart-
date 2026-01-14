using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SupplierModel;
using Dapper;
using Ecommerce.Interface;

namespace Ecommerce.Repository.Admin
{
    public class SupplierRepository : ISupplierInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public SupplierRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<Supplier>> GetSupplierListAsync(SupplierListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<Supplier, SupplierListInput>(
                    storedProcedureName: "ProSupplierListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching supplier list.", ex);
            }
        }

        public async Task<Supplier> GetSupplierByIdAsync(long id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    var supplier = await connection.QueryFirstOrDefaultAsync<Supplier>(
                        "ProSupplierSelectById",
                        new { ID_Supplier = id },
                        commandType: System.Data.CommandType.StoredProcedure
                    );
                    return supplier;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching supplier by ID.", ex);
            }
        }

        public async Task<CommonResponse> CreateSupplierAsync(SupplierUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<SupplierUpdateInput>(
                    storedProcedureName: "ProSupplierUpdate",
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
                    ResponseMsg = $"An error occurred while creating supplier: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdateSupplierAsync(SupplierUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<SupplierUpdateInput>(
                    storedProcedureName: "ProSupplierUpdate",
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
                    ResponseMsg = $"An error occurred while updating supplier: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeleteSupplierAsync(SupplierUpdateInput input)
        {
            try
            {
                input.UserAction = 3; // 3 = Delete (soft delete)
                var response = await _dataAccessDapper.ExecuteStoredProcedure<SupplierUpdateInput>(
                    storedProcedureName: "ProSupplierUpdate",
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
                    ResponseMsg = $"An error occurred while deleting supplier: {ex.Message}"
                };
            }
        }
    }
}

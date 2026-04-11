using Ecommerce.Helpers.Purchases;
using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.PurchaseModel;
using Dapper;
using System.Data;

namespace Ecommerce.Repository.Admin
{
    public class PurchaseRepository : IPurchaseInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public PurchaseRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<Purchase>> GetPurchaseListAsync(PurchaseListInput input)
        {
            if (input == null)
            {
                return PurchaseHelper.EmptyTableOutput(null);
            }

            try
            {
                var spParams = PurchaseHelper.BuildPurchaseListSpParameters(input);
                return await _dataAccessDapper.GetMultipleListByStoredProcedure<Purchase>(
                    storedProcedureName: "ProPurchaseListSelect",
                    parameter: spParams);
            }
            catch
            {
                return PurchaseHelper.EmptyTableOutput(input);
            }
        }

        public async Task<PurchaseDetailFull> GetPurchaseByIdAsync(int id)
        {
            try
            {
                using (var connection = _dataAccessDapper.CreateConnection())
                {
                    using (var multi = await connection.QueryMultipleAsync(
                        "ProPurchaseDetailSelect",
                        new { ID_Purchase = id },
                        commandType: CommandType.StoredProcedure
                    ))
                    {
                        var purchaseHeader = await multi.ReadFirstOrDefaultAsync<Purchase>();
                        var purchaseDetails = (await multi.ReadAsync<PurchaseDetail>()).ToList();
                        var stockBatches = (await multi.ReadAsync<Stock>()).ToList();

                        return new PurchaseDetailFull
                        {
                            PurchaseHeader = purchaseHeader ?? new Purchase(),
                            PurchaseDetails = purchaseDetails,
                            StockBatches = stockBatches
                        };
                    }
                }
            }
            catch
            {
                return new PurchaseDetailFull
                {
                    PurchaseHeader = new Purchase(),
                    PurchaseDetails = new List<PurchaseDetail>(),
                    StockBatches = new List<Stock>()
                };
            }
        }

        public async Task<CommonResponse> CreatePurchaseAsync(PurchaseUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<PurchaseUpdateInput>(
                    storedProcedureName: "ProPurchaseUpdate",
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
                    ResponseMsg = $"An error occurred while creating purchase: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdatePurchaseAsync(PurchaseUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<PurchaseUpdateInput>(
                    storedProcedureName: "ProPurchaseUpdate",
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
                    ResponseMsg = $"An error occurred while updating purchase: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeletePurchaseAsync(PurchaseDeleteInput input)
        {
            try
            {
                input.UserAction = 3; // 3 = Delete
                // For delete, we need to use ProPurchaseUpdate with UserAction = 3
                var updateInput = new PurchaseUpdateInput
                {
                    UserAction = 3,
                    ID_Purchase = input.ID_Purchase,
                    FK_Supplier = 0,
                    PurchaseDate = null,
                    InvoiceNumber = string.Empty,
                    Notes = string.Empty,
                    PurchaseDetails = string.Empty, // Not needed for delete
                    EnterBy = input.EnterBy,
                    CancelledReason = input.CancelledReason
                };
                var response = await _dataAccessDapper.ExecuteStoredProcedure<PurchaseUpdateInput>(
                    storedProcedureName: "ProPurchaseUpdate",
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
                    ResponseMsg = $"An error occurred while deleting purchase: {ex.Message}"
                };
            }
        }
    }
}

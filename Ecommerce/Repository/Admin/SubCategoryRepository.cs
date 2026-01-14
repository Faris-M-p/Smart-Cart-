using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SubCategoryModel;

namespace Ecommerce.Repository.Admin
{
    public class SubCategoryRepository : ISubCategoryInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public SubCategoryRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<SubCategory>> GetSubCategoryListAsync(SubCategoryListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<SubCategory, SubCategoryListInput>(
                    storedProcedureName: "ProSubCategoryListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching subcategory list.", ex);
            }
        }

        public async Task<CommonResponse> CreateSubCategoryAsync(SubCategoryUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<SubCategoryUpdateInput>(
                    storedProcedureName: "ProSubCategoryUpdate",
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
                    ResponseMsg = $"An error occurred while creating subcategory: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdateSubCategoryAsync(SubCategoryUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<SubCategoryUpdateInput>(
                    storedProcedureName: "ProSubCategoryUpdate",
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
                    ResponseMsg = $"An error occurred while updating subcategory: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeleteSubCategoryAsync(SubCategoryDeleteInput input)
        {
            try
            {
                var response = await _dataAccessDapper.ExecuteStoredProcedure<SubCategoryDeleteInput>(
                    storedProcedureName: "ProSubCategoryDelete",
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
                    ResponseMsg = $"An error occurred while deleting subcategory: {ex.Message}"
                };
            }
        }
    }
}

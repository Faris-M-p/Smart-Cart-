using Ecommerce.Interface;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.CategoryModel;

namespace Ecommerce.Repository.Admin
{
    public class CategoryRepository : ICategoryInterface
    {
        private readonly IDataAccessDapper _dataAccessDapper;

        public CategoryRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<Category>> GetCategoryListAsync(CategoryListInput input)
        {
            try
            {
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<Category, CategoryListInput>(
                    storedProcedureName: "ProCategoryListSelect",
                    parameter: input
                );
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching category list.", ex);
            }
        }

        public async Task<CommonResponse> CreateCategoryAsync(CategoryUpdateInput input)
        {
            try
            {
                input.UserAction = 1; // 1 = Add
                var response = await _dataAccessDapper.ExecuteStoredProcedure<CategoryUpdateInput>(
                    storedProcedureName: "ProCategoryUpdate",
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
                    ResponseMsg = $"An error occurred while creating category: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> UpdateCategoryAsync(CategoryUpdateInput input)
        {
            try
            {
                input.UserAction = 2; // 2 = Edit
                var response = await _dataAccessDapper.ExecuteStoredProcedure<CategoryUpdateInput>(
                    storedProcedureName: "ProCategoryUpdate",
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
                    ResponseMsg = $"An error occurred while updating category: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponse> DeleteCategoryAsync(CategoryDeleteInput input)
        {
            try
            {
                var response = await _dataAccessDapper.ExecuteStoredProcedure<CategoryDeleteInput>(
                    storedProcedureName: "ProCategoryDelete",
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
                    ResponseMsg = $"An error occurred while deleting category: {ex.Message}"
                };
            }
        }
    }
}

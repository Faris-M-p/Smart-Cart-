using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.CategoryModel;

namespace Ecommerce.Interface.Admin
{
    public interface ICategoryInterface
    {
        Task<List<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<TableOutput<Category>> GetCategoryListAsync(CategoryListInput input);
        Task<CommonResponse> CreateCategoryAsync(CategoryUpdateInput input);
        Task<CommonResponse> UpdateCategoryAsync(CategoryUpdateInput input);
        Task<CommonResponse> DeleteCategoryAsync(CategoryDeleteInput input);
    }
}

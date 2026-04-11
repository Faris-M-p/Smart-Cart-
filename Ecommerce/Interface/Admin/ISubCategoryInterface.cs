using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SubCategoryModel;

namespace Ecommerce.Interface.Admin
{
    public interface ISubCategoryInterface
    {
        Task<SubCategory?> GetSubCategoryByIdAsync(int id);
        Task<TableOutput<SubCategory>> GetSubCategoryListAsync(SubCategoryListInput input);
        Task<CommonResponse> CreateSubCategoryAsync(SubCategoryUpdateInput input);
        Task<CommonResponse> UpdateSubCategoryAsync(SubCategoryUpdateInput input);
        Task<CommonResponse> DeleteSubCategoryAsync(SubCategoryDeleteInput input);
    }
}

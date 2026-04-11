using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantModel;

namespace Ecommerce.Interface.Admin
{
    public interface IVariantInterface
    {
        Task<TableOutput<VariantListOutput>> GetVariantListAsync(VariantListInput input);
        Task<CommonResponse> CreateVariantAsync(VariantUpdateInput input);
        Task<CommonResponse> UpdateVariantAsync(VariantUpdateInput input);
        Task<CommonResponse> DeleteVariantAsync(VariantDeleteInput input);
        Task<VariantSelectByIdOutput?> GetVariantByIdAsync(int id);
    }
}

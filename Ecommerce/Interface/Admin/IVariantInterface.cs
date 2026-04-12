using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantModel;

namespace Ecommerce.Interface.Admin
{
    public interface IVariantInterface
    {
        Task<TableOutput<Variant>> GetVariantListAsync(VariantListInput input);
        Task<Variant?> GetVariantByIdAsync(int id);
        Task<CommonResponse> CreateVariantAsync(VariantUpdateInput input);
        Task<CommonResponse> UpdateVariantAsync(VariantUpdateInput input);
        Task<CommonResponse> DeleteVariantAsync(VariantDeleteInput input);
    }
}

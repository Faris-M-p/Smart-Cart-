using static Ecommerce.Models.Admin.VariantValueModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface.Admin
{
    public interface IVariantValueInterface
    {
        Task<TableOutput<VariantValue>> GetVariantValueListAsync(VariantValueListInput input);

        Task<List<VariantValue>> GetByVariantIdAsync(int variantId);

        Task<VariantValue?> GetVariantValueByIdAsync(int id);

        Task<CommonResponse> CreateVariantValueAsync(VariantValueCreateInput input);

        Task<CommonResponse> UpdateVariantValueAsync(VariantValueUpdateInput input);

        Task<CommonResponse> DeleteVariantValueAsync(VariantValueDeleteInput input);
    }
}

using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantValueModel;

namespace Ecommerce.Interface.Admin
{
    public interface IVariantValueInterface
    {
        Task<TableOutput<VariantValueListOutput>> GetVariantValueListAsync(VariantValueListInput input);
        Task<CommonResponse> UpdateVariantValueAsync(VariantValueUpdateInput input);
        Task<VariantValueSelectByIdOutput?> GetVariantValueByIdAsync(int id);
    }
}

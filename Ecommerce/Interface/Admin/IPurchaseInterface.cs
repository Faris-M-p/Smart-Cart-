using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.PurchaseModel;

namespace Ecommerce.Interface.Admin
{
    public interface IPurchaseInterface
    {
        Task<TableOutput<Purchase>> GetPurchaseListAsync(PurchaseListInput input);
        Task<PurchaseDetailFull> GetPurchaseByIdAsync(int id);
        Task<CommonResponse> CreatePurchaseAsync(PurchaseUpdateInput input);
        Task<CommonResponse> UpdatePurchaseAsync(PurchaseUpdateInput input);
        Task<CommonResponse> DeletePurchaseAsync(PurchaseDeleteInput input);
    }
}

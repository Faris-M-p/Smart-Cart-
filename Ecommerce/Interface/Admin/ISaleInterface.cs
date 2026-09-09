using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SaleModel;

namespace Ecommerce.Interface.Admin
{
    public interface ISaleInterface
    {
        Task<TableOutput<Sale>> GetSaleListAsync(SaleListInput input);
        Task<SaleDetailFull> GetSaleByIdAsync(int id);
        Task<List<SaleSkuOption>> GetSkuOptionsAsync(int productId);
        Task<CommonResponse> CreateSaleAsync(SaleUpdateInput input);
        Task<CommonResponse> UpdateSaleAsync(SaleUpdateInput input);
        Task<CommonResponse> DeleteSaleAsync(SaleDeleteInput input);
    }
}

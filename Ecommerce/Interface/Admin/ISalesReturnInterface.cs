using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SaleModel;
using static Ecommerce.Models.Admin.SalesReturnModel;

namespace Ecommerce.Interface.Admin
{
    public interface ISalesReturnInterface
    {
        Task<TableOutput<SalesReturn>> GetSalesReturnListAsync(SalesReturnListInput input);
        Task<SalesReturnDetailFull> GetSalesReturnByIdAsync(int id);
        Task<List<SaleLookup>> GetSaleLookupsAsync();
        Task<List<SaleDetail>> GetReturnableLinesAsync(int saleId);
        Task<CommonResponse> CreateSalesReturnAsync(SalesReturnUpdateInput input);
        Task<CommonResponse> DeleteSalesReturnAsync(SalesReturnDeleteInput input);
    }
}

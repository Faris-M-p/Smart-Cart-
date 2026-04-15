using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.InventoryModel;

namespace Ecommerce.Interface.Admin
{
    public interface IInventoryInterface
    {
        Task<TableOutput<StockRow>> GetStockListAsync(StockListInput input);
        Task<CommonResponse> AdjustStockAsync(AdjustStockInput input);
    }
}


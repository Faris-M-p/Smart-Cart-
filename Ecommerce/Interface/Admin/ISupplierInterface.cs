using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SupplierModel;

namespace Ecommerce.Interface.Admin
{
    public interface ISupplierInterface
    {
        Task<TableOutput<Supplier>> GetSupplierListAsync(SupplierListInput input);
        Task<Supplier> GetSupplierByIdAsync(long id);
        Task<CommonResponse> CreateSupplierAsync(SupplierUpdateInput input);
        Task<CommonResponse> UpdateSupplierAsync(SupplierUpdateInput input);
        Task<CommonResponse> DeleteSupplierAsync(SupplierUpdateInput input);
    }
}

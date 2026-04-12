using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.BrandModel;

namespace Ecommerce.Interface.Admin
{
    public interface IBrandInterface
    {
        Task<TableOutput<Brand>> GetBrandListAsync(BrandListInput input);
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<CommonResponse> CreateBrandAsync(BrandUpdateInput input);
        Task<CommonResponse> UpdateBrandAsync(BrandUpdateInput input);
        Task<CommonResponse> DeleteBrandAsync(BrandDeleteInput input);
    }
}

using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductVariantModel;

namespace Ecommerce.Interface.Admin
{
    public interface IProductVariantInterface
    {
        Task<TableOutput<ProductVariant>> GetProductVariantListAsync(ProductVariantListInput input);
        Task<ProductVariantDetail?> GetProductVariantByIdAsync(int id);
        Task<CommonResponse> CreateProductVariantAsync(ProductVariantUpdateInput input);
        Task<CommonResponse> UpdateProductVariantAsync(ProductVariantUpdateInput input);
        Task<CommonResponse> DeleteProductVariantAsync(ProductVariantDeleteInput input);
    }
}

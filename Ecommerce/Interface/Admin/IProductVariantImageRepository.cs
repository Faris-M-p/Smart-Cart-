using Ecommerce.Models.Entities;

namespace Ecommerce.Interface.Admin
{
    public interface IProductVariantImageRepository
    {
        Task<bool> ProductVariantExistsAsync(int skuId);
        Task<List<ProductVariantImageEntity>> GetBySkuIdAsync(int skuId);
        Task<ProductVariantImageEntity?> GetByIdAsync(int imageId);
        Task<int> GetImageCountAsync(int skuId);
        Task AddRangeAsync(IEnumerable<ProductVariantImageEntity> images);
        Task RemoveAsync(ProductVariantImageEntity image);
        Task SaveChangesAsync();
    }
}

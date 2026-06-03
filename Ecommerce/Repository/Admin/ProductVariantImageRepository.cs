using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.Admin
{
    public class ProductVariantImageRepository : IProductVariantImageRepository
    {
        private readonly EcommerceDbContext _dbContext;

        public ProductVariantImageRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ProductVariantExistsAsync(int skuId) =>
            _dbContext.ProductVariants.AnyAsync(x => x.ID_ProductVariant == skuId && !x.Cancelled);

        public Task<List<ProductVariantImageEntity>> GetBySkuIdAsync(int skuId) =>
            _dbContext.ProductVariantImages
                .Where(x => x.FK_ProductVariant == skuId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ID_ProductVariantImage)
                .ToListAsync();

        public Task<ProductVariantImageEntity?> GetByIdAsync(int imageId) =>
            _dbContext.ProductVariantImages.FirstOrDefaultAsync(x => x.ID_ProductVariantImage == imageId);

        public Task<int> GetImageCountAsync(int skuId) =>
            _dbContext.ProductVariantImages.CountAsync(x => x.FK_ProductVariant == skuId);

        public async Task AddRangeAsync(IEnumerable<ProductVariantImageEntity> images)
        {
            await _dbContext.ProductVariantImages.AddRangeAsync(images);
        }

        public Task RemoveAsync(ProductVariantImageEntity image)
        {
            _dbContext.ProductVariantImages.Remove(image);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
    }
}

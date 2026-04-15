using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DataAccess
{
    public class EcommerceDbContext : DbContext
    {
        public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
            : base(options)
        {
        }

        public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

        public DbSet<SubCategoryEntity> SubCategories => Set<SubCategoryEntity>();

        public DbSet<BrandEntity> Brands => Set<BrandEntity>();

        public DbSet<ProductEntity> Products => Set<ProductEntity>();

        public DbSet<ProductImageEntity> ProductImages => Set<ProductImageEntity>();

        public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();

        public DbSet<VariantEntity> Variants => Set<VariantEntity>();

        public DbSet<VariantValueEntity> VariantValues => Set<VariantValueEntity>();

        public DbSet<StockEntity> Stock => Set<StockEntity>();

        public DbSet<PurchaseEntity> Purchases => Set<PurchaseEntity>();

        public DbSet<PurchaseDetailEntity> PurchaseDetails => Set<PurchaseDetailEntity>();

        public DbSet<ProductVariantEntity> ProductVariants => Set<ProductVariantEntity>();

        public DbSet<ProductVariantAttributeEntity> ProductVariantAttributes => Set<ProductVariantAttributeEntity>();
    }
}

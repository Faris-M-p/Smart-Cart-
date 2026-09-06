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

        public DbSet<ProductMediaEntity> ProductMedia => Set<ProductMediaEntity>();

        public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();

        public DbSet<VariantEntity> Variants => Set<VariantEntity>();

        public DbSet<VariantValueEntity> VariantValues => Set<VariantValueEntity>();

        public DbSet<StockEntity> Stock => Set<StockEntity>();

        public DbSet<PurchaseEntity> Purchases => Set<PurchaseEntity>();

        public DbSet<PurchaseDetailEntity> PurchaseDetails => Set<PurchaseDetailEntity>();

        public DbSet<ProductVariantEntity> ProductVariants => Set<ProductVariantEntity>();

        public DbSet<ProductVariantAttributeEntity> ProductVariantAttributes => Set<ProductVariantAttributeEntity>();

        public DbSet<ProductVariantImageEntity> ProductVariantImages => Set<ProductVariantImageEntity>();

        public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();

        public DbSet<ModuleEntity> Modules => Set<ModuleEntity>();

        public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();

        public DbSet<UserRolePermissionEntity> UserRolePermissions => Set<UserRolePermissionEntity>();

        public DbSet<AdminUserEntity> AdminUsers => Set<AdminUserEntity>();
    }
}

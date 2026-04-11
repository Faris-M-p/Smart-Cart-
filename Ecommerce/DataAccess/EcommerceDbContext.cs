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
    }
}

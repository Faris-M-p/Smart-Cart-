using Microsoft.EntityFrameworkCore;
using SmartCartPro.Entities;

namespace SmartCartPro.Data;

/// <summary>
/// EF Core mappings aligned with Database-Postgres SQL (source of truth).
/// Do not add migrations; do not call EnsureCreated / EnsureDeleted / Database.Migrate.
/// </summary>
public class SmartCartDbContext : DbContext
{
    public SmartCartDbContext(DbContextOptions<SmartCartDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserStoreAccess> UserStoreAccess => Set<UserStoreAccess>();
    public DbSet<AuthOtpChallenge> AuthOtpChallenges => Set<AuthOtpChallenge>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.HasIndex(x => x.CompanyCode).IsUnique();
        });

        modelBuilder.Entity<Store>(e =>
        {
            e.HasOne(x => x.Company)
                .WithMany(c => c.Stores)
                .HasForeignKey(x => x.FkCompany)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkCompany, x.StoreCode }).IsUnique();
        });

        modelBuilder.Entity<SubCategory>(e =>
        {
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.FkCategory)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.FkCategory)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.ProductCode).IsUnique();
        });

        modelBuilder.Entity<Inventory>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkStore);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkStore, x.OrderNumber }).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<AuthOtpChallenge>(e =>
        {
            e.HasIndex(x => x.ChallengeToken).IsUnique();
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.FkUser)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserStoreAccess>(e =>
        {
            e.HasOne(x => x.User)
                .WithMany(u => u.StoreAccess)
                .HasForeignKey(x => x.FkUser)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkUser, x.FkStore }).IsUnique();
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(x => x.RoleCode).IsUnique();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasOne(x => x.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(x => x.FkUser)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.FkRole)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkUser, x.FkRole }).IsUnique();
        });
    }
}

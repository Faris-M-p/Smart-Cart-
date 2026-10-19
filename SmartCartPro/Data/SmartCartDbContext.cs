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

    // ── Phase 3: Catalog ─────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    // ── Phase 4: Inventory ────────────────────────────────────────
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // ── Phase 5: Purchasing ───────────────────────────────────────
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseDetail> PurchaseDetails => Set<PurchaseDetail>();

    // ── Phase 6: POS ──────────────────────────────────────────────
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnDetail> SalesReturnDetails => Set<SalesReturnDetail>();

    // ── Online Orders (Phase 7) ───────────────────────────────────
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OnlineOrderItem> OnlineOrderItems => Set<OnlineOrderItem>();

    // ── Phase 7: Reviews, Cart, Wishlist, Settings ────────────────
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Setting> Settings => Set<Setting>();

    // ── Platform / super-admin entities ──────────────────────────
    public DbSet<License> Licenses => Set<License>();
    public DbSet<InstallationConfig> InstallationConfigs => Set<InstallationConfig>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<ModulePreset> ModulePresets => Set<ModulePreset>();
    public DbSet<ModulePresetItem> ModulePresetItems => Set<ModulePresetItem>();
    public DbSet<InstallationModule> InstallationModules => Set<InstallationModule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<InvoiceSequence> InvoiceSequences => Set<InvoiceSequence>();

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

        // ── New super-admin entities ─────────────────────────────────────────

        modelBuilder.Entity<License>(e =>
        {
            e.HasIndex(x => x.LicenseKey).IsUnique();
        });

        modelBuilder.Entity<InstallationConfig>(e =>
        {
            e.HasOne(x => x.License)
                .WithMany()
                .HasForeignKey(x => x.FkLicense)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Module>(e =>
        {
            e.HasIndex(x => x.ModuleCode).IsUnique();
        });

        modelBuilder.Entity<ModulePreset>(e =>
        {
            e.HasIndex(x => x.PresetCode).IsUnique();
        });

        modelBuilder.Entity<ModulePresetItem>(e =>
        {
            e.HasOne(x => x.Preset)
                .WithMany(p => p.Items)
                .HasForeignKey(x => x.FkPreset)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Module)
                .WithMany()
                .HasForeignKey(x => x.FkModule)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkPreset, x.FkModule }).IsUnique();
        });

        modelBuilder.Entity<InstallationModule>(e =>
        {
            e.HasOne(x => x.Module)
                .WithMany()
                .HasForeignKey(x => x.FkModule)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkModule).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.FkUser)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => new { x.EntityName, x.EntityId });
        });

        modelBuilder.Entity<UserInvitation>(e =>
        {
            e.HasIndex(x => x.InvitationToken).IsUnique();
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.FkUser)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.FkRole)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasIndex(x => x.PermissionCode).IsUnique();
            e.HasOne(x => x.Module)
                .WithMany()
                .HasForeignKey(x => x.FkModule)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.FkRole)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.FkPermission)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkRole, x.FkPermission }).IsUnique();
        });

        modelBuilder.Entity<InvoiceSequence>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkStore, x.SequenceType }).IsUnique();
        });

        // ── Phase 3: Catalog ─────────────────────────────────────────

        modelBuilder.Entity<Brand>(e =>
        {
            e.HasIndex(x => x.BrandName).IsUnique();
        });

        modelBuilder.Entity<Unit>(e =>
        {
            e.HasIndex(x => x.UnitCode).IsUnique();
        });

        modelBuilder.Entity<Tax>(e =>
        {
            e.HasIndex(x => x.TaxCode).IsUnique();
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SkuCode).IsUnique().HasFilter("\"skucode\" IS NOT NULL");
            e.HasIndex(x => x.Barcode).IsUnique().HasFilter("\"barcode\" IS NOT NULL");
            e.HasIndex(x => new { x.FkProduct, x.VariantName }).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.FkCategory)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.ProductCode).IsUnique();
            e.HasIndex(x => x.Sku).IsUnique().HasFilter("\"sku\" IS NOT NULL");
            e.HasIndex(x => x.Barcode).IsUnique().HasFilter("\"barcode\" IS NOT NULL");
        });

        // ── Phase 4: Inventory ─────────────────────────────────────────

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
            e.HasIndex(x => x.FkProduct);
        });

        modelBuilder.Entity<StockMovement>(e =>
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
            e.HasIndex(x => x.FkProduct);
            e.HasIndex(x => x.CreatedAt);
        });

        // ── Phase 5: Purchasing ────────────────────────────────────────

        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasIndex(x => x.SupplierName).IsUnique();
        });

        modelBuilder.Entity<Purchase>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.FkSupplier)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkStore);
            e.HasIndex(x => x.PurchaseDate);
        });

        modelBuilder.Entity<PurchaseDetail>(e =>
        {
            e.HasOne(x => x.Purchase)
                .WithMany(p => p.Details)
                .HasForeignKey(x => x.FkPurchase)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkPurchase);
        });

        // ── Phase 6: POS ───────────────────────────────────────────────

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique().HasFilter("\"email\" IS NOT NULL");
            e.HasIndex(x => x.Phone).IsUnique().HasFilter("\"phone\" IS NOT NULL");
            e.HasIndex(x => x.FullName);
        });

        modelBuilder.Entity<CashSession>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.FkUser)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkStore, x.Status });
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.FkCustomer)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CashSession)
                .WithMany()
                .HasForeignKey(x => x.FkCashSession)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.FkStore, x.InvoiceNumber }).IsUnique();
            e.HasIndex(x => x.SaleDate);
        });

        modelBuilder.Entity<SaleDetail>(e =>
        {
            e.HasOne(x => x.Sale)
                .WithMany(s => s.Details)
                .HasForeignKey(x => x.FkSale)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkSale);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(x => x.FkSale)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CashSession)
                .WithMany()
                .HasForeignKey(x => x.FkCashSession)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.FkStore);
            e.HasIndex(x => x.FkSale);
        });

        modelBuilder.Entity<SalesReturn>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Sale)
                .WithMany()
                .HasForeignKey(x => x.FkSale)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkStore, x.ReturnNumber }).IsUnique();
        });

        modelBuilder.Entity<SalesReturnDetail>(e =>
        {
            e.HasOne(x => x.SalesReturn)
                .WithMany(r => r.Details)
                .HasForeignKey(x => x.FkSalesReturn)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkSalesReturn);
        });

        // ── Phase 7: Online Order Items ────────────────────────────────

        modelBuilder.Entity<OnlineOrderItem>(e =>
        {
            e.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.FkOnlineOrder)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkOnlineOrder);
            e.HasIndex(x => x.FkStore);
        });

        // ── Phase 7: Product Reviews ───────────────────────────────────

        modelBuilder.Entity<ProductReview>(e =>
        {
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.FkCustomer)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkProduct);
            // Unique: one review per customer per order item (enforced via patch column fk_onlineorderitem)
            e.HasIndex(x => new { x.FkCustomer, x.FkOnlineOrderItem })
                .IsUnique()
                .HasFilter("\"fk_onlineorderitem\" IS NOT NULL");
        });

        // ── Phase 7: Cart & CartItem ───────────────────────────────────

        modelBuilder.Entity<Cart>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.FkCustomer)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.FkStore, x.FkCustomer });
            e.HasIndex(x => new { x.FkStore, x.SessionKey });
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasOne(x => x.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(x => x.FkCart)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.FkStore);
            e.HasIndex(x => new { x.FkCart, x.FkProduct, x.FkProductVariant }).IsUnique();
        });

        // ── Phase 7: Wishlist & WishlistItem ──────────────────────────

        modelBuilder.Entity<Wishlist>(e =>
        {
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.FkCustomer)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkStore, x.FkCustomer }).IsUnique();
        });

        modelBuilder.Entity<WishlistItem>(e =>
        {
            e.HasOne(x => x.Wishlist)
                .WithMany(w => w.Items)
                .HasForeignKey(x => x.FkWishlist)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.FkProduct)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkWishlist, x.FkProduct }).IsUnique();
            e.HasIndex(x => x.FkStore);
        });

        // ── Phase 7: Settings ──────────────────────────────────────────

        modelBuilder.Entity<Setting>(e =>
        {
            e.HasIndex(x => new { x.SettingScope, x.FkStore, x.SettingKey }).IsUnique();
        });
    }
}

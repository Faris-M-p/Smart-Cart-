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
            e.ToTable("company");
            e.HasKey(x => x.IdCompany);
            e.Property(x => x.IdCompany).HasColumnName("id_company");
            e.Property(x => x.CompanyCode).HasColumnName("companycode").IsRequired();
            e.Property(x => x.CompanyName).HasColumnName("companyname").IsRequired();
            e.Property(x => x.LegalName).HasColumnName("legalname");
            e.Property(x => x.TaxNumber).HasColumnName("taxnumber");
            e.Property(x => x.RegistrationNo).HasColumnName("registrationno");
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.Phone).HasColumnName("phone");
            e.Property(x => x.Website).HasColumnName("website");
            e.Property(x => x.AddressLine1).HasColumnName("addressline1");
            e.Property(x => x.AddressLine2).HasColumnName("addressline2");
            e.Property(x => x.City).HasColumnName("city");
            e.Property(x => x.State).HasColumnName("state");
            e.Property(x => x.PostalCode).HasColumnName("postalcode");
            e.Property(x => x.Country).HasColumnName("country");
            e.Property(x => x.CurrencyCode).HasColumnName("currencycode").HasMaxLength(3).IsRequired();
            e.Property(x => x.TimeZone).HasColumnName("timezone").IsRequired();
            e.Property(x => x.Locale).HasColumnName("locale").IsRequired();
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasIndex(x => x.CompanyCode).IsUnique();
        });

        modelBuilder.Entity<Store>(e =>
        {
            e.ToTable("store");
            e.HasKey(x => x.IdStore);
            e.Property(x => x.IdStore).HasColumnName("id_store");
            e.Property(x => x.FkCompany).HasColumnName("fk_company").IsRequired();
            e.Property(x => x.StoreCode).HasColumnName("storecode").IsRequired();
            e.Property(x => x.StoreName).HasColumnName("storename").IsRequired();
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.Phone).HasColumnName("phone");
            e.Property(x => x.AddressLine1).HasColumnName("addressline1");
            e.Property(x => x.AddressLine2).HasColumnName("addressline2");
            e.Property(x => x.City).HasColumnName("city");
            e.Property(x => x.State).HasColumnName("state");
            e.Property(x => x.PostalCode).HasColumnName("postalcode");
            e.Property(x => x.Country).HasColumnName("country");
            e.Property(x => x.IsDefault).HasColumnName("isdefault");
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasOne(x => x.Company)
                .WithMany(c => c.Stores)
                .HasForeignKey(x => x.FkCompany)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkCompany, x.StoreCode }).IsUnique();
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.IdCategory);
            e.Property(x => x.IdCategory).HasColumnName("id_category");
            e.Property(x => x.CategoryCode).HasColumnName("categorycode");
            e.Property(x => x.CategoryName).HasColumnName("categoryname").IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.DisplayOrder).HasColumnName("displayorder");
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
        });

        modelBuilder.Entity<SubCategory>(e =>
        {
            e.ToTable("subcategories");
            e.HasKey(x => x.IdSubcategory);
            e.Property(x => x.IdSubcategory).HasColumnName("id_subcategory");
            e.Property(x => x.FkCategory).HasColumnName("fk_category").IsRequired();
            e.Property(x => x.SubCategoryName).HasColumnName("subcategoryname").IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.DisplayOrder).HasColumnName("displayorder");
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.FkCategory)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(x => x.IdProduct);
            e.Property(x => x.IdProduct).HasColumnName("id_product");
            e.Property(x => x.FkCategory).HasColumnName("fk_category");
            e.Property(x => x.FkSubcategory).HasColumnName("fk_subcategory");
            e.Property(x => x.FkBrand).HasColumnName("fk_brand");
            e.Property(x => x.FkUnit).HasColumnName("fk_unit");
            e.Property(x => x.FkTax).HasColumnName("fk_tax");
            e.Property(x => x.ProductCode).HasColumnName("productcode").IsRequired();
            e.Property(x => x.ProductName).HasColumnName("productname").IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Sku).HasColumnName("sku");
            e.Property(x => x.Barcode).HasColumnName("barcode");
            e.Property(x => x.CostPrice).HasColumnName("costprice").HasPrecision(12, 2);
            e.Property(x => x.SellingPrice).HasColumnName("sellingprice").HasPrecision(12, 2);
            e.Property(x => x.Mrp).HasColumnName("mrp").HasPrecision(12, 2);
            e.Property(x => x.ReorderLevel).HasColumnName("reorderlevel").HasPrecision(12, 3);
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.IsOnline).HasColumnName("isonline");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.FkCategory)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.ProductCode).IsUnique();
        });

        modelBuilder.Entity<Inventory>(e =>
        {
            e.ToTable("inventory");
            e.HasKey(x => x.IdInventory);
            e.Property(x => x.IdInventory).HasColumnName("id_inventory");
            e.Property(x => x.FkStore).HasColumnName("fk_store").IsRequired();
            e.Property(x => x.FkProduct).HasColumnName("fk_product").IsRequired();
            e.Property(x => x.FkProductVariant).HasColumnName("fk_productvariant");
            e.Property(x => x.QuantityOnHand).HasColumnName("quantityonhand").HasPrecision(14, 3);
            e.Property(x => x.QuantityReserved).HasColumnName("quantityreserved").HasPrecision(14, 3);
            e.Property(x => x.ReorderLevel).HasColumnName("reorderlevel").HasPrecision(14, 3);
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
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
            e.ToTable("online_orders");
            e.HasKey(x => x.IdOnlineOrder);
            e.Property(x => x.IdOnlineOrder).HasColumnName("id_onlineorder");
            e.Property(x => x.FkStore).HasColumnName("fk_store").IsRequired();
            e.Property(x => x.FkCustomer).HasColumnName("fk_customer").IsRequired();
            e.Property(x => x.OrderNumber).HasColumnName("ordernumber").IsRequired();
            e.Property(x => x.OrderDate).HasColumnName("orderdate");
            e.Property(x => x.Status).HasColumnName("status").IsRequired();
            e.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2);
            e.Property(x => x.TaxAmount).HasColumnName("taxamount").HasPrecision(12, 2);
            e.Property(x => x.ShippingAmount).HasColumnName("shippingamount").HasPrecision(12, 2);
            e.Property(x => x.DiscountAmount).HasColumnName("discountamount").HasPrecision(12, 2);
            e.Property(x => x.TotalAmount).HasColumnName("totalamount").HasPrecision(12, 2);
            e.Property(x => x.ShippingAddress).HasColumnName("shippingaddress");
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.FkStore)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FkStore, x.OrderNumber }).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.IdUser);
            e.Property(x => x.IdUser).HasColumnName("id_user");
            e.Property(x => x.UserName).HasColumnName("username").IsRequired();
            e.Property(x => x.Email).HasColumnName("email").IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("passwordhash").IsRequired();
            e.Property(x => x.FullName).HasColumnName("fullname").IsRequired();
            e.Property(x => x.Phone).HasColumnName("phone");
            e.Property(x => x.EmployeeCode).HasColumnName("employeecode");
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.MustChangePassword).HasColumnName("mustchangepassword");
            e.Property(x => x.LastLoginAt).HasColumnName("lastloginat");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<UserStoreAccess>(e =>
        {
            e.ToTable("user_store_access");
            e.HasKey(x => x.IdUserStoreAccess);
            e.Property(x => x.IdUserStoreAccess).HasColumnName("id_userstoreaccess");
            e.Property(x => x.FkUser).HasColumnName("fk_user").IsRequired();
            e.Property(x => x.FkStore).HasColumnName("fk_store").IsRequired();
            e.Property(x => x.IsDefault).HasColumnName("isdefault");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
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
            e.ToTable("roles");
            e.HasKey(x => x.IdRole);
            e.Property(x => x.IdRole).HasColumnName("id_role");
            e.Property(x => x.RoleCode).HasColumnName("rolecode").IsRequired();
            e.Property(x => x.RoleName).HasColumnName("rolename").IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsSystem).HasColumnName("issystem");
            e.Property(x => x.IsActive).HasColumnName("isactive");
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
            e.Property(x => x.Cancelled).HasColumnName("cancelled");
            e.Property(x => x.CancelledOn).HasColumnName("cancelledon");
            e.Property(x => x.CancelledReason).HasColumnName("cancelledreason");
            e.HasIndex(x => x.RoleCode).IsUnique();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => x.IdUserRole);
            e.Property(x => x.IdUserRole).HasColumnName("id_userrole");
            e.Property(x => x.FkUser).HasColumnName("fk_user").IsRequired();
            e.Property(x => x.FkRole).HasColumnName("fk_role").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("createdat");
            e.Property(x => x.UpdatedAt).HasColumnName("updatedat");
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

using Microsoft.EntityFrameworkCore;
using SmartCart.Domain.Platform;

namespace SmartCart.Infrastructure.Persistence;

public sealed class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
    {
    }

    public DbSet<CompanyAccount> CompanyAccounts => Set<CompanyAccount>();
    public DbSet<ClientDatabase> ClientDatabases => Set<ClientDatabase>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<CompanySubscription> CompanySubscriptions => Set<CompanySubscription>();
    public DbSet<ModuleDefinition> ModuleDefinitions => Set<ModuleDefinition>();
    public DbSet<CompanyModule> CompanyModules => Set<CompanyModule>();
    public DbSet<ThemeDefinition> ThemeDefinitions => Set<ThemeDefinition>();
    public DbSet<CompanyTheme> CompanyThemes => Set<CompanyTheme>();
    public DbSet<HomepageSectionType> HomepageSectionTypes => Set<HomepageSectionType>();
    public DbSet<CompanySectionAllowance> CompanySectionAllowances => Set<CompanySectionAllowance>();
    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();
    public DbSet<PlatformAuditLog> PlatformAuditLogs => Set<PlatformAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyAccount>(e =>
        {
            e.ToTable("company_accounts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired();
            e.Property(x => x.StoreMode).HasMaxLength(16).IsRequired();
        });

        modelBuilder.Entity<ClientDatabase>(e =>
        {
            e.ToTable("client_databases");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CompanyAccountId).IsUnique();
            e.HasIndex(x => x.DatabaseName).IsUnique();
            e.Property(x => x.DatabaseName).HasMaxLength(128).IsRequired();
            e.Property(x => x.EncryptedConnectionString).IsRequired();
            e.HasOne(x => x.CompanyAccount).WithOne(x => x.Database)
                .HasForeignKey<ClientDatabase>(x => x.CompanyAccountId);
        });

        modelBuilder.Entity<SubscriptionPlan>(e =>
        {
            e.ToTable("subscription_plans");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<CompanySubscription>(e =>
        {
            e.ToTable("company_subscriptions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CompanyAccountId).IsUnique();
            e.HasOne(x => x.CompanyAccount).WithOne(x => x.Subscription)
                .HasForeignKey<CompanySubscription>(x => x.CompanyAccountId);
            e.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.SubscriptionPlanId);
        });

        modelBuilder.Entity<ModuleDefinition>(e =>
        {
            e.ToTable("module_definitions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<CompanyModule>(e =>
        {
            e.ToTable("company_modules");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CompanyAccountId, x.ModuleDefinitionId }).IsUnique();
            e.HasOne(x => x.CompanyAccount).WithMany(x => x.Modules)
                .HasForeignKey(x => x.CompanyAccountId);
            e.HasOne(x => x.Module).WithMany().HasForeignKey(x => x.ModuleDefinitionId);
        });

        modelBuilder.Entity<ThemeDefinition>(e =>
        {
            e.ToTable("theme_definitions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<CompanyTheme>(e =>
        {
            e.ToTable("company_themes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CompanyAccountId).IsUnique();
            e.HasOne(x => x.CompanyAccount).WithOne(x => x.Theme)
                .HasForeignKey<CompanyTheme>(x => x.CompanyAccountId);
            e.HasOne(x => x.Theme).WithMany().HasForeignKey(x => x.ThemeDefinitionId);
        });

        modelBuilder.Entity<HomepageSectionType>(e =>
        {
            e.ToTable("homepage_section_types");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<CompanySectionAllowance>(e =>
        {
            e.ToTable("company_section_allowances");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CompanyAccountId, x.HomepageSectionTypeId }).IsUnique();
            e.HasOne(x => x.CompanyAccount).WithMany(x => x.SectionAllowances)
                .HasForeignKey(x => x.CompanyAccountId);
            e.HasOne(x => x.SectionType).WithMany().HasForeignKey(x => x.HomepageSectionTypeId);
        });

        modelBuilder.Entity<PlatformUser>(e =>
        {
            e.ToTable("platform_users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<PlatformAuditLog>(e =>
        {
            e.ToTable("platform_audit_logs");
            e.HasKey(x => x.Id);
        });
    }
}

namespace SmartCart.Domain.Client;

/// <summary>Single company row inside a client database (database-per-company).</summary>
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Store> Stores { get; set; } = new List<Store>();
}

public class Store
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}

public class Employee
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<EmployeeStoreAccess> StoreAccess { get; set; } = new List<EmployeeStoreAccess>();
}

public class EmployeeStoreAccess
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int StoreId { get; set; }
    public Store Store { get; set; } = null!;
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Sku { get; set; } = string.Empty;
    public string? Label { get; set; }
    public decimal SellingPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Store-scoped stock quantity for a SKU.</summary>
public class Stock
{
    public long Id { get; set; }
    public int StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public decimal Quantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Reason { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class HomepageSection
{
    public int Id { get; set; }
    public string SectionTypeCode { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsPublished { get; set; }
    public string ConfigJson { get; set; } = "{}";
}

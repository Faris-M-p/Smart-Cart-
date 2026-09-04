SET NOCOUNT ON;
GO

PRINT 'Seeding Permissions...';
GO

;WITH SeedPermissions AS (
    SELECT * FROM (VALUES
        -- Dashboard
        (N'Dashboard',       N'View',        N'Dashboard.View',        10),

        -- Categories
        (N'Categories',      N'View',        N'Categories.View',       10),
        (N'Categories',      N'Create',      N'Categories.Create',     20),
        (N'Categories',      N'Edit',        N'Categories.Edit',       30),
        (N'Categories',      N'Delete',      N'Categories.Delete',     40),

        -- SubCategories
        (N'SubCategories',   N'View',        N'SubCategories.View',    10),
        (N'SubCategories',   N'Create',      N'SubCategories.Create',  20),
        (N'SubCategories',   N'Edit',        N'SubCategories.Edit',    30),
        (N'SubCategories',   N'Delete',      N'SubCategories.Delete',  40),

        -- Brands
        (N'Brands',          N'View',        N'Brands.View',           10),
        (N'Brands',          N'Create',      N'Brands.Create',         20),
        (N'Brands',          N'Edit',        N'Brands.Edit',           30),
        (N'Brands',          N'Delete',      N'Brands.Delete',         40),

        -- Products
        (N'Products',        N'View',        N'Products.View',         10),
        (N'Products',        N'Create',      N'Products.Create',       20),
        (N'Products',        N'Edit',        N'Products.Edit',         30),
        (N'Products',        N'Delete',      N'Products.Delete',       40),

        -- Variants
        (N'Variants',        N'View',        N'Variants.View',         10),
        (N'Variants',        N'Create',      N'Variants.Create',       20),
        (N'Variants',        N'Edit',        N'Variants.Edit',         30),
        (N'Variants',        N'Delete',      N'Variants.Delete',       40),

        -- VariantValues
        (N'VariantValues',   N'View',        N'VariantValues.View',    10),
        (N'VariantValues',   N'Create',      N'VariantValues.Create',  20),
        (N'VariantValues',   N'Edit',        N'VariantValues.Edit',    30),
        (N'VariantValues',   N'Delete',      N'VariantValues.Delete',  40),

        -- ProductVariants (SKU)
        (N'ProductVariants', N'View',        N'ProductVariants.View',  10),
        (N'ProductVariants', N'Create',      N'ProductVariants.Create',20),
        (N'ProductVariants', N'Edit',        N'ProductVariants.Edit',  30),
        (N'ProductVariants', N'Delete',      N'ProductVariants.Delete',40),

        -- Suppliers
        (N'Suppliers',       N'View',        N'Suppliers.View',        10),
        (N'Suppliers',       N'Create',      N'Suppliers.Create',      20),
        (N'Suppliers',       N'Edit',        N'Suppliers.Edit',        30),
        (N'Suppliers',       N'Delete',      N'Suppliers.Delete',      40),

        -- Purchases
        (N'Purchases',       N'View',        N'Purchases.View',        10),
        (N'Purchases',       N'Create',      N'Purchases.Create',      20),
        (N'Purchases',       N'Edit',        N'Purchases.Edit',        30),
        (N'Purchases',       N'Delete',      N'Purchases.Delete',      40),

        -- Stock
        (N'Stock',           N'View',        N'Stock.View',            10),
        (N'Stock',           N'Adjust',      N'Stock.Adjust',          20),

        -- Orders
        (N'Orders',          N'View',        N'Orders.View',           10),
        (N'Orders',          N'Create',      N'Orders.Create',         20),
        (N'Orders',          N'Edit',        N'Orders.Edit',           30),
        (N'Orders',          N'Cancel',      N'Orders.Cancel',         40),

        -- Billing
        (N'Billing',         N'View',        N'Billing.View',          10),
        (N'Billing',         N'Create',      N'Billing.Create',        20),
        (N'Billing',         N'Print',       N'Billing.Print',         30),
        (N'Billing',         N'Cancel',      N'Billing.Cancel',        40),

        -- Employees
        (N'Employees',       N'View',        N'Employees.View',        10),
        (N'Employees',       N'Create',      N'Employees.Create',      20),
        (N'Employees',       N'Edit',        N'Employees.Edit',        30),
        (N'Employees',       N'Deactivate',  N'Employees.Deactivate',  40),

        -- UserRoles
        (N'UserRoles',       N'View',        N'UserRoles.View',        10),
        (N'UserRoles',       N'Create',      N'UserRoles.Create',      20),
        (N'UserRoles',       N'Edit',        N'UserRoles.Edit',        30),
        (N'UserRoles',       N'Delete',      N'UserRoles.Delete',      40)
    ) AS v(ModuleName, PermissionName, PermissionCode, DisplayOrder)
)
INSERT INTO [dbo].[Permissions] (
    [FK_Module], [PermissionName], [PermissionCode], [DisplayOrder],
    [IsActive], [CreatedAt], [Cancelled]
)
SELECT
    m.ID_Module, s.PermissionName, s.PermissionCode, s.DisplayOrder,
    1, GETDATE(), 0
FROM SeedPermissions s
INNER JOIN [dbo].[Modules] m ON m.ModuleName = s.ModuleName
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[Permissions] p
    WHERE p.PermissionCode = s.PermissionCode
);
GO

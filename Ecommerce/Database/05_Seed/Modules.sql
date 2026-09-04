SET NOCOUNT ON;
GO

PRINT 'Seeding Modules...';
GO

;WITH SeedModules AS (
    SELECT * FROM (VALUES
        (N'Dashboard',        N'Dashboard',         10),
        (N'Categories',       N'Categories',        20),
        (N'SubCategories',    N'Sub Categories',    30),
        (N'Brands',           N'Brands',            40),
        (N'Products',         N'Products',          50),
        (N'Variants',         N'Variants',          60),
        (N'VariantValues',    N'Variant Values',    70),
        (N'ProductVariants',  N'Product Variants',  80),
        (N'Suppliers',        N'Suppliers',         90),
        (N'Purchases',        N'Purchases',        100),
        (N'Stock',            N'Stock',            110),
        (N'Orders',           N'Orders',           120),
        (N'Billing',          N'Billing',          130),
        (N'Employees',        N'Employees',        140),
        (N'UserRoles',        N'User Roles',       150)
    ) AS v(ModuleName, DisplayName, DisplayOrder)
)
INSERT INTO [dbo].[Modules] (
    [ModuleName], [DisplayName], [DisplayOrder],
    [IsActive], [CreatedAt], [Cancelled]
)
SELECT
    s.ModuleName, s.DisplayName, s.DisplayOrder,
    1, GETDATE(), 0
FROM SeedModules s
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[Modules] m
    WHERE m.ModuleName = s.ModuleName
);
GO

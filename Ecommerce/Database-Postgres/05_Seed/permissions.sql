\echo 'Seeding permissions...'

INSERT INTO permissions (fk_module, permissionname, permissioncode, displayorder, isactive, createdat, cancelled)
SELECT m.id_module, s.permissionname, s.permissioncode, s.displayorder, TRUE, NOW(), FALSE
FROM (VALUES
    ('Dashboard',       'View',        'Dashboard.View',        10),
    ('Categories',      'View',        'Categories.View',       10),
    ('Categories',      'Create',      'Categories.Create',     20),
    ('Categories',      'Edit',        'Categories.Edit',       30),
    ('Categories',      'Delete',      'Categories.Delete',     40),
    ('SubCategories',   'View',        'SubCategories.View',    10),
    ('SubCategories',   'Create',      'SubCategories.Create',  20),
    ('SubCategories',   'Edit',        'SubCategories.Edit',    30),
    ('SubCategories',   'Delete',      'SubCategories.Delete',  40),
    ('Brands',          'View',        'Brands.View',           10),
    ('Brands',          'Create',      'Brands.Create',         20),
    ('Brands',          'Edit',        'Brands.Edit',           30),
    ('Brands',          'Delete',      'Brands.Delete',         40),
    ('Products',        'View',        'Products.View',         10),
    ('Products',        'Create',      'Products.Create',       20),
    ('Products',        'Edit',        'Products.Edit',         30),
    ('Products',        'Delete',      'Products.Delete',       40),
    ('Variants',        'View',        'Variants.View',         10),
    ('Variants',        'Create',      'Variants.Create',       20),
    ('Variants',        'Edit',        'Variants.Edit',         30),
    ('Variants',        'Delete',      'Variants.Delete',       40),
    ('VariantValues',   'View',        'VariantValues.View',    10),
    ('VariantValues',   'Create',      'VariantValues.Create',  20),
    ('VariantValues',   'Edit',        'VariantValues.Edit',    30),
    ('VariantValues',   'Delete',      'VariantValues.Delete',  40),
    ('ProductVariants', 'View',        'ProductVariants.View',  10),
    ('ProductVariants', 'Create',      'ProductVariants.Create',20),
    ('ProductVariants', 'Edit',        'ProductVariants.Edit',  30),
    ('ProductVariants', 'Delete',      'ProductVariants.Delete',40),
    ('Suppliers',       'View',        'Suppliers.View',        10),
    ('Suppliers',       'Create',      'Suppliers.Create',      20),
    ('Suppliers',       'Edit',        'Suppliers.Edit',        30),
    ('Suppliers',       'Delete',      'Suppliers.Delete',      40),
    ('Purchases',       'View',        'Purchases.View',        10),
    ('Purchases',       'Create',      'Purchases.Create',      20),
    ('Purchases',       'Edit',        'Purchases.Edit',        30),
    ('Purchases',       'Delete',      'Purchases.Delete',      40),
    ('Stock',           'View',        'Stock.View',            10),
    ('Stock',           'Adjust',      'Stock.Adjust',          20),
    ('Orders',          'View',        'Orders.View',           10),
    ('Orders',          'Create',      'Orders.Create',         20),
    ('Orders',          'Edit',        'Orders.Edit',           30),
    ('Orders',          'Cancel',      'Orders.Cancel',         40),
    ('Sales',           'View',        'Sales.View',            10),
    ('Sales',           'Create',      'Sales.Create',          20),
    ('Sales',           'Edit',        'Sales.Edit',            30),
    ('Sales',           'Delete',      'Sales.Delete',          40),
    ('SalesReturns',    'View',        'SalesReturns.View',     10),
    ('SalesReturns',    'Create',      'SalesReturns.Create',   20),
    ('SalesReturns',    'Delete',      'SalesReturns.Delete',   30),
    ('Billing',         'View',        'Billing.View',          10),
    ('Billing',         'Create',      'Billing.Create',        20),
    ('Billing',         'Print',       'Billing.Print',         30),
    ('Billing',         'Cancel',      'Billing.Cancel',        40),
    ('Employees',       'View',        'Employees.View',        10),
    ('Employees',       'Create',      'Employees.Create',      20),
    ('Employees',       'Edit',        'Employees.Edit',        30),
    ('Employees',       'Deactivate',  'Employees.Deactivate',  40),
    ('UserRoles',       'View',        'UserRoles.View',        10),
    ('UserRoles',       'Create',      'UserRoles.Create',      20),
    ('UserRoles',       'Edit',        'UserRoles.Edit',        30),
    ('UserRoles',       'Delete',      'UserRoles.Delete',      40)
) AS s(modulename, permissionname, permissioncode, displayorder)
INNER JOIN modules m ON m.modulename = s.modulename
WHERE NOT EXISTS (
    SELECT 1 FROM permissions p WHERE p.permissioncode = s.permissioncode
);

-- Remove leftover store-wide Settings (online sell is per product/sku)
DELETE FROM userrolepermissions urp
USING permissions p
WHERE p.id_permission = urp.fk_permission
  AND p.permissioncode IN ('Settings.View', 'Settings.Edit');

DELETE FROM permissions
WHERE permissioncode IN ('Settings.View', 'Settings.Edit');

DELETE FROM modules
WHERE modulename = 'Settings';

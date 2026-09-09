\echo 'Seeding modules...'

INSERT INTO modules (module_name, display_name, display_order, is_active, created_at, cancelled)
SELECT s.module_name, s.display_name, s.display_order, TRUE, NOW(), FALSE
FROM (VALUES
    ('Dashboard',        'Dashboard',         10),
    ('Categories',       'Categories',        20),
    ('SubCategories',    'Sub Categories',    30),
    ('Brands',           'Brands',            40),
    ('Products',         'Products',          50),
    ('Variants',         'Variants',          60),
    ('VariantValues',    'Variant Values',    70),
    ('ProductVariants',  'Product Variants',  80),
    ('Suppliers',        'Suppliers',         90),
    ('Purchases',        'Purchases',        100),
    ('Stock',            'Stock',            110),
    ('Orders',           'Orders',           120),
    ('Billing',          'Billing',          130),
    ('Employees',        'Employees',        140),
    ('UserRoles',        'User Roles',       150)
) AS s(module_name, display_name, display_order)
WHERE NOT EXISTS (
    SELECT 1 FROM modules m WHERE m.module_name = s.module_name
);

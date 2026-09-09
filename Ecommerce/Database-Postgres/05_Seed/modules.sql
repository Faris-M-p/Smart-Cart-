\echo 'Seeding modules...'

INSERT INTO modules (modulename, displayname, displayorder, isactive, createdat, cancelled)
SELECT s.modulename, s.displayname, s.displayorder, TRUE, NOW(), FALSE
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
) AS s(modulename, displayname, displayorder)
WHERE NOT EXISTS (
    SELECT 1 FROM modules m WHERE m.modulename = s.modulename
);

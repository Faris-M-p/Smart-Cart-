-- Default modules + permissions (repeatable)

INSERT INTO modules (modulecode, modulename, displayorder, isactive)
SELECT v.modulecode, v.modulename, v.displayorder, TRUE
FROM (VALUES
    ('DASHBOARD', 'Dashboard', 10),
    ('PRODUCTS', 'Products', 20),
    ('INVENTORY', 'Inventory', 30),
    ('PURCHASES', 'Purchases', 40),
    ('POS', 'Point of Sale', 50),
    ('CUSTOMERS', 'Customers', 60),
    ('REPORTS', 'Reports', 70),
    ('SETTINGS', 'Settings', 80),
    ('USERS', 'Users & Roles', 90)
) AS v(modulecode, modulename, displayorder)
WHERE NOT EXISTS (
    SELECT 1 FROM modules m WHERE m.modulecode = v.modulecode AND m.cancelled = FALSE
);

INSERT INTO permissions (fk_module, permissioncode, permissionname, displayorder, isactive)
SELECT m.id_module, v.permissioncode, v.permissionname, v.displayorder, TRUE
FROM (VALUES
    ('DASHBOARD', 'dashboard.view', 'View dashboard', 1),
    ('PRODUCTS', 'products.view', 'View products', 1),
    ('PRODUCTS', 'products.manage', 'Manage products', 2),
    ('INVENTORY', 'inventory.view', 'View inventory', 1),
    ('INVENTORY', 'inventory.adjust', 'Adjust stock', 2),
    ('PURCHASES', 'purchases.view', 'View purchases', 1),
    ('PURCHASES', 'purchases.manage', 'Manage purchases', 2),
    ('POS', 'pos.sell', 'Create POS sales', 1),
    ('POS', 'pos.return', 'Process sales returns', 2),
    ('POS', 'pos.cancel', 'Cancel POS invoices', 3),
    ('CUSTOMERS', 'customers.view', 'View customers', 1),
    ('CUSTOMERS', 'customers.manage', 'Manage customers', 2),
    ('REPORTS', 'reports.view', 'View reports', 1),
    ('REPORTS', 'reports.profit', 'View cost and profit on reports', 2),
    ('SETTINGS', 'settings.manage', 'Manage settings', 1),
    ('USERS', 'users.manage', 'Manage users and roles', 1)
) AS v(modulecode, permissioncode, permissionname, displayorder)
JOIN modules m ON m.modulecode = v.modulecode AND m.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM permissions p WHERE p.permissioncode = v.permissioncode AND p.cancelled = FALSE
);

-- Grant every permission to the company Admin role (idempotent).
INSERT INTO role_permissions (fk_role, fk_permission)
SELECT r.id_role, p.id_permission
FROM roles r
CROSS JOIN permissions p
WHERE r.rolecode = 'ADMIN'
  AND r.cancelled = FALSE
  AND p.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM role_permissions rp
      WHERE rp.fk_role = r.id_role AND rp.fk_permission = p.id_permission
  );

-- Default least-privilege grants for the built-in employee roles.
INSERT INTO role_permissions (fk_role, fk_permission)
SELECT r.id_role, p.id_permission
FROM (VALUES
    ('STORE_MANAGER', 'dashboard.view'),
    ('STORE_MANAGER', 'products.view'), ('STORE_MANAGER', 'products.manage'),
    ('STORE_MANAGER', 'inventory.view'), ('STORE_MANAGER', 'inventory.adjust'),
    ('STORE_MANAGER', 'purchases.view'), ('STORE_MANAGER', 'purchases.manage'),
    ('STORE_MANAGER', 'pos.sell'), ('STORE_MANAGER', 'pos.return'), ('STORE_MANAGER', 'pos.cancel'),
    ('STORE_MANAGER', 'customers.view'), ('STORE_MANAGER', 'customers.manage'),
    ('STORE_MANAGER', 'reports.view'),
    ('CASHIER', 'pos.sell'),
    ('INVENTORY', 'inventory.view'), ('INVENTORY', 'inventory.adjust'),
    ('INVENTORY', 'products.view'), ('INVENTORY', 'purchases.view'),
    ('VIEWER', 'reports.view'), ('VIEWER', 'dashboard.view'),
    ('VIEWER', 'products.view'), ('VIEWER', 'inventory.view')
) AS g(rolecode, permissioncode)
JOIN roles r ON r.rolecode = g.rolecode AND r.cancelled = FALSE
JOIN permissions p ON p.permissioncode = g.permissioncode AND p.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM role_permissions rp
    WHERE rp.fk_role = r.id_role AND rp.fk_permission = p.id_permission
);

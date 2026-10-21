-- Default roles (repeatable / idempotent)
\set ON_ERROR_STOP on

INSERT INTO roles (rolecode, rolename, description, issystem, isactive)
SELECT v.rolecode, v.rolename, v.description, TRUE, TRUE
FROM (VALUES
    ('ADMIN', 'Admin', 'Company administrator. Uses every module the software provider has enabled, and manages employees and roles.'),
    ('STORE_MANAGER', 'Store Manager', 'Manage a store'),
    ('CASHIER', 'Cashier', 'POS billing'),
    ('INVENTORY', 'Inventory Clerk', 'Stock and purchasing'),
    ('VIEWER', 'Viewer', 'Read-only reporting')
) AS v(rolecode, rolename, description)
WHERE NOT EXISTS (
    SELECT 1 FROM roles r WHERE r.rolecode = v.rolecode AND r.cancelled = FALSE
);

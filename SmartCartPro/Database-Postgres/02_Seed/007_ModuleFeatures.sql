-- Required feature/menu catalog, installation defaults, and preset feature membership.
\set ON_ERROR_STOP on

-- Seed descriptive metadata for the final modules table.
UPDATE modules AS m
SET is_core = v.is_core,
    description = v.description,
    updatedat = NOW()
FROM (VALUES
    ('DASHBOARD',  TRUE,  'Home metrics and shortcuts for the signed-in store.'),
    ('PRODUCTS',   TRUE,  'Catalog: products, categories, brands, units and taxes.'),
    ('INVENTORY',  FALSE, 'Stock on hand, adjustments, transfers and history.'),
    ('PURCHASES',  FALSE, 'Supplier bills, receiving and purchase documents.'),
    ('POS',        FALSE, 'In-store sales, returns, invoices and cash sessions.'),
    ('CUSTOMERS',  FALSE, 'Customer list and profiles used by POS and online orders.'),
    ('REPORTS',    FALSE, 'Sales, stock, tax and purchase reports.'),
    ('SETTINGS',   TRUE,  'Company profile, stores and system settings.'),
    ('USERS',      TRUE,  'Employees, roles and permission assignment.')
) AS v(modulecode, is_core, description)
WHERE m.modulecode = v.modulecode
  AND m.cancelled = FALSE;

-- ---------------------------------------------------------------------------

-- Groups (level 2). State is derived from children. Not stored as enabled.
-- ---------------------------------------------------------------------------
INSERT INTO module_features (
    fk_module, fk_parent_feature, featurecode, featurename, description, is_core, displayorder, isactive)
SELECT m.id_module, NULL, v.featurecode, v.featurename, v.description, FALSE, v.displayorder, TRUE
FROM (VALUES
    ('REPORTS', 'REPORTS.SALES',    'Sales reports',         'Bills, items, discounts and cashier totals.', 20),
    ('REPORTS', 'REPORTS.STOCK',    'Stock reports',         'On-hand stock, ledgers and movement.', 30),
    ('REPORTS', 'REPORTS.PURCHASE', 'Purchase reports',      'Purchase bills and supplier or item totals.', 40),
    ('REPORTS', 'REPORTS.PARTIES',  'Parties and masters',   'Customer report and master lists.', 70)
) AS v(modulecode, featurecode, featurename, description, displayorder)
JOIN modules m ON m.modulecode = v.modulecode AND m.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM module_features f WHERE f.featurecode = v.featurecode AND f.cancelled = FALSE
);

-- ---------------------------------------------------------------------------
-- Leaves directly under a module
-- ---------------------------------------------------------------------------
INSERT INTO module_features (
    fk_module, fk_parent_feature, featurecode, featurename, description, is_core, displayorder, isactive)
SELECT m.id_module, NULL, v.featurecode, v.featurename, v.description, v.is_core, v.displayorder, TRUE
FROM (VALUES
    ('DASHBOARD', 'DASHBOARD.OVERVIEW',  'Dashboard overview', 'Signed-in home page.', TRUE, 10),
    ('DASHBOARD', 'DASHBOARD.METRICS',   'Store metrics',      'Store KPIs, trends and the live snapshot.', TRUE, 20),
    ('DASHBOARD', 'DASHBOARD.SHORTCUTS', 'Quick shortcuts',    'Links to frequent tasks.', TRUE, 30),

    ('PRODUCTS', 'PRODUCTS.CATEGORIES',    'Categories',     'Category master.', TRUE, 10),
    ('PRODUCTS', 'PRODUCTS.SUBCATEGORIES', 'Sub-categories', 'Sub-category master.', TRUE, 20),
    ('PRODUCTS', 'PRODUCTS.BRANDS',        'Brands',         'Brand master.', FALSE, 30),
    ('PRODUCTS', 'PRODUCTS.UNITS',         'Units',          'Units of measure.', TRUE, 40),
    ('PRODUCTS', 'PRODUCTS.TAXES',         'Taxes',          'Tax master.', TRUE, 50),
    ('PRODUCTS', 'PRODUCTS.PRODUCTS',      'Products',       'Product list, create, edit and details.', TRUE, 60),
    ('PRODUCTS', 'PRODUCTS.VARIANTS',      'SKU / Variants', 'Variant list and create or edit.', FALSE, 70),
    ('PRODUCTS', 'PRODUCTS.REVIEWS',       'Reviews',        'Review list and approve or hide.', FALSE, 80),

    ('INVENTORY', 'INVENTORY.STOCK',     'Stock overview', 'On-hand stock for the current store.', TRUE, 10),
    ('INVENTORY', 'INVENTORY.LOW_STOCK', 'Low stock',      'Items at or below reorder level.', FALSE, 20),
    ('INVENTORY', 'INVENTORY.HISTORY',   'Stock history',  'Stock movement history.', FALSE, 30),
    ('INVENTORY', 'INVENTORY.OPENING',   'Opening stock',  'Record opening quantities.', FALSE, 40),
    ('INVENTORY', 'INVENTORY.ADJUST',    'Adjust stock',   'Correct on-hand quantity.', FALSE, 50),
    ('INVENTORY', 'INVENTORY.TRANSFER',  'Transfer',       'Move stock between stores.', FALSE, 60),

    ('PURCHASES', 'PURCHASES.SUPPLIERS', 'Suppliers',          'Supplier master.', TRUE, 10),
    ('PURCHASES', 'PURCHASES.ORDERS',    'Purchase orders',    'Purchase list, create, edit, print and cancel.', TRUE, 20),
    ('PURCHASES', 'PURCHASES.RECEIVING', 'Purchase receiving', 'Receive goods against a purchase.', FALSE, 30),

    ('POS', 'POS.BILLING',       'POS billing',   'New sale, barcode lookup and payment on the bill.', TRUE, 10),
    ('POS', 'POS.CASH_SESSION',  'Cash session',  'Open, close and review cashier sessions.', FALSE, 20),
    ('POS', 'POS.INVOICES',      'Invoices',      'Invoice list, details, print and cancel.', TRUE, 30),
    ('POS', 'POS.RETURNS',       'Returns',       'Sales returns.', FALSE, 40),
    ('POS', 'POS.ONLINE_ORDERS', 'Online orders', 'Online order list, details and status.', FALSE, 50),

    ('CUSTOMERS', 'CUSTOMERS.MANAGE', 'Customers', 'Customer list, create and edit.', TRUE, 10),

    ('REPORTS', 'REPORTS.HUB',          'Reports hub',    'The reports home page.', TRUE, 10),
    ('REPORTS', 'REPORTS.TAX.GST',      'GST summary',    'GST summary and the earlier tax report.', FALSE, 50),
    ('REPORTS', 'REPORTS.PROFIT.MARGIN','Profit / margin','Profit and margin report.', FALSE, 60),

    ('SETTINGS', 'SETTINGS.SETTINGS', 'Settings',   'System settings.', TRUE, 10),
    ('SETTINGS', 'SETTINGS.COMPANY',  'Company',    'Company profile.', TRUE, 20),
    ('SETTINGS', 'SETTINGS.STORES',   'Stores',     'Store list, create and edit.', TRUE, 30),
    ('SETTINGS', 'SETTINGS.AUDIT',    'Audit log',  'Audit log viewer.', FALSE, 40),

    ('USERS', 'USERS.EMPLOYEES', 'Employees', 'Employee accounts.', TRUE, 10),
    ('USERS', 'USERS.ROLES',     'Roles',     'Roles and their permissions.', TRUE, 20)
) AS v(modulecode, featurecode, featurename, description, is_core, displayorder)
JOIN modules m ON m.modulecode = v.modulecode AND m.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM module_features f WHERE f.featurecode = v.featurecode AND f.cancelled = FALSE
);

-- ---------------------------------------------------------------------------
-- Leaves under a group
-- ---------------------------------------------------------------------------
INSERT INTO module_features (
    fk_module, fk_parent_feature, featurecode, featurename, description, is_core, displayorder, isactive)
SELECT m.id_module, parent.id_feature, v.featurecode, v.featurename, v.description, FALSE, v.displayorder, TRUE
FROM (VALUES
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.BILL_WISE', 'Sales bill-wise',        'Bill-wise sales and the earlier sales report.', 10),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.ITEM',      'Item-wise sales',        'Item sales and the earlier products report.', 20),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.CATEGORY',  'Category-wise sales',    'Sales grouped by category.', 30),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.RETURNS',   'Sales returns',          'Sales return report.', 40),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.DAY_CLOSE', 'Day close / cashier',    'Cashier day-close report.', 50),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.DISCOUNT',  'Discount report',        'Discounts given on bills and items.', 60),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.CANCELLED', 'Cancelled bills',        'Bills cancelled in the selected dates.', 70),
    ('REPORTS', 'REPORTS.SALES', 'REPORTS.SALES.HOURLY',    'Hourly sales',           'Sales by hour or day of week.', 80),

    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.VALUATION',    'Stock valuation / current stock', 'Current stock and the earlier stock valuation report.', 10),
    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.ITEM_LEDGER',  'Item ledger',                     'Movements for one item.', 20),
    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.STOCK_LEDGER', 'Stock ledger',                    'Opening, in, out and closing by item.', 30),
    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.MOVEMENT',     'Stock inward/outward',            'Movement totals or the document list.', 40),
    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.NON_MOVING',   'Non-moving items',                'Items not sold in the selected days.', 50),
    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.LOW_STOCK',    'Low stock / reorder',             'Low-stock report.', 60),
    ('REPORTS', 'REPORTS.STOCK', 'REPORTS.STOCK.ADJUSTMENTS',  'Adjustments & transfers',         'Adjustment and transfer movements.', 70),

    ('REPORTS', 'REPORTS.PURCHASE', 'REPORTS.PURCHASE.BILLS',    'Purchase bills',          'Purchase bills and the earlier purchases report.', 10),
    ('REPORTS', 'REPORTS.PURCHASE', 'REPORTS.PURCHASE.ANALYSIS', 'Supplier / item purchase','Purchase totals by supplier or item.', 20),

    ('REPORTS', 'REPORTS.PARTIES', 'REPORTS.PARTIES.CUSTOMERS', 'Customer report', 'The earlier customers report.', 10),
    ('REPORTS', 'REPORTS.PARTIES', 'REPORTS.PARTIES.MASTERS',   'Master lists',    'Customers, suppliers, employees and categories.', 20)
) AS v(modulecode, parentcode, featurecode, featurename, description, displayorder)
JOIN modules m ON m.modulecode = v.modulecode AND m.cancelled = FALSE
JOIN module_features parent ON parent.featurecode = v.parentcode AND parent.cancelled = FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM module_features f WHERE f.featurecode = v.featurecode AND f.cancelled = FALSE
);

-- ---------------------------------------------------------------------------
-- Core modules are always installed. Existing optional modules stay as they are.
-- ---------------------------------------------------------------------------
INSERT INTO installation_modules (fk_module, createdat)
SELECT m.id_module, NOW()
FROM modules m
WHERE m.is_core = TRUE
  AND m.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM installation_modules im WHERE im.fk_module = m.id_module
  );

-- Every leaf of an enabled module is on, so an existing install keeps the same screens.
INSERT INTO installation_features (fk_feature, createdat)
SELECT f.id_feature, NOW()
FROM installation_modules im
JOIN module_features f ON f.fk_module = im.fk_module
WHERE f.cancelled = FALSE
  AND f.isactive = TRUE
  AND NOT EXISTS (
      SELECT 1 FROM module_features child
      WHERE child.fk_parent_feature = f.id_feature
        AND child.cancelled = FALSE
  )
  AND NOT EXISTS (
      SELECT 1 FROM installation_features ix WHERE ix.fk_feature = f.id_feature
  );

-- ---------------------------------------------------------------------------

-- Basic: core leaves of its modules, plus the named optional leaves.
INSERT INTO module_preset_features (fk_preset, fk_feature)
SELECT p.id_preset, f.id_feature
FROM module_presets p
JOIN module_features f ON f.cancelled = FALSE
WHERE p.presetcode = 'BASIC'
  AND f.featurecode IN (
        'DASHBOARD.OVERVIEW','DASHBOARD.METRICS','DASHBOARD.SHORTCUTS',
        'PRODUCTS.CATEGORIES','PRODUCTS.SUBCATEGORIES','PRODUCTS.UNITS','PRODUCTS.TAXES','PRODUCTS.PRODUCTS',
        'PRODUCTS.BRANDS',
        'INVENTORY.STOCK','INVENTORY.LOW_STOCK','INVENTORY.OPENING','INVENTORY.ADJUST',
        'POS.BILLING','POS.INVOICES','POS.CASH_SESSION','POS.RETURNS',
        'SETTINGS.SETTINGS','SETTINGS.COMPANY','SETTINGS.STORES',
        'USERS.EMPLOYEES','USERS.ROLES'
      )
  AND NOT EXISTS (
      SELECT 1 FROM module_preset_features x
      WHERE x.fk_preset = p.id_preset AND x.fk_feature = f.id_feature
  );

-- Premium: every leaf (rows that are not parents).
INSERT INTO module_preset_features (fk_preset, fk_feature)
SELECT p.id_preset, f.id_feature
FROM module_presets p
JOIN module_features f ON f.cancelled = FALSE AND f.isactive = TRUE
WHERE p.presetcode = 'PREMIUM'
  AND NOT EXISTS (
      SELECT 1 FROM module_features child
      WHERE child.fk_parent_feature = f.id_feature
        AND child.cancelled = FALSE
  )
  AND NOT EXISTS (
      SELECT 1 FROM module_preset_features x
      WHERE x.fk_preset = p.id_preset AND x.fk_feature = f.id_feature
  );

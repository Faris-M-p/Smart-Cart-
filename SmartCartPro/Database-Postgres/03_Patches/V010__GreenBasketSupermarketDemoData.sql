-- =============================================================================
-- V010__GreenBasketSupermarketDemoData.sql
-- Green Basket Supermarket demo data for manual admin and POS testing.
--
-- NOT EXECUTED by the author. Run it yourself after V001–V009 and the
-- 02_Seed scripts. Safe to run again: if setting gb.demo.dataset exists,
-- the script inserts nothing and commits.
--
-- Does not TRUNCATE, drop, or update existing business rows.
-- Does not change passwords of users that already exist.
-- Does not raise maxstores and does not insert a second store, so there
-- are no inter-store transfers.
-- Does not seed OTP challenges, JWTs, or payment-provider references.
--
-- Variant model actually in this database:
--   product_variants has variantname, skucode, barcode, additionalprice.
--   There is no variants / variantvalues / productvariantattributes table.
--   Colour and size below are names on the SKU, not selectable axes.
--   Selling price of a SKU is products.sellingprice + additionalprice.
--   Cost and MRP live on the product. A larger pack stores its extra
--   selling price in additionalprice. Purchase lines store that pack's cost.
--   Stock is inventory (fk_store, fk_product, fk_productvariant).
--   Parents that have variants get no inventory row.
--
-- Test employee sign-in (development only):
--   Password hash is the Identity V3 hash already stored in
--   02_Seed/004_PlatformSuperAdmin.sql. mustchangepassword is true.
--   These accounts are not platform super admins.
--   Customer passwordhash is left null. There is no customer login seed.
--
-- Stock rule used here, matching OnlineOrderRepository:
--   Pending orders do not move stock.
--   Confirmed, packed, shipped and delivered deduct once (movement sale).
--   A cancel after confirm writes sale and return_in, which net to zero.
--   A cancel while still pending writes nothing.
--   Packed / shipped / delivered do not deduct a second time.
--   Draft and posted purchases do not move stock.
--   Received quantity is the purchase movement, not the ordered quantity.
-- =============================================================================
\set ON_ERROR_STOP on

BEGIN;

DO $gb$
DECLARE
    v_company   INT;
    v_store     INT;
    v_session   INT;
    v_admin     INT;
    v_neg       INT;
    v_parent    INT;
    v_hash      TEXT := 'AQAAAAIAAYagAAAAEMwD8hjWbMf/QYpdRJNr7Lh4y+wGZTz0+4bgKBektuHXhV1ZqgL/KONn5CMWhSCZXQ==';
BEGIN
    IF EXISTS (
        SELECT 1 FROM settings
        WHERE setting_scope = 'company'
          AND fk_store IS NULL
          AND settingkey = 'gb.demo.dataset'
    ) THEN
        RAISE NOTICE 'Green Basket demo dataset is already loaded. No rows were inserted.';
        RETURN;
    END IF;

    -- Masters the app expects. Insert only when that code is missing.
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
        SELECT 1 FROM modules m
        WHERE m.modulecode = v.modulecode AND m.cancelled = FALSE
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
        ('CUSTOMERS', 'customers.view', 'View customers', 1),
        ('CUSTOMERS', 'customers.manage', 'Manage customers', 2),
        ('REPORTS', 'reports.view', 'View reports', 1),
        ('SETTINGS', 'settings.manage', 'Manage settings', 1),
        ('USERS', 'users.manage', 'Manage users and roles', 1)
    ) AS v(modulecode, permissioncode, permissionname, displayorder)
    JOIN modules m ON m.modulecode = v.modulecode AND m.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM permissions p
        WHERE p.permissioncode = v.permissioncode AND p.cancelled = FALSE
    );

    INSERT INTO permissions (fk_module, permissioncode, permissionname, displayorder, isactive)
    SELECT m.id_module, 'pos.cancel', 'Cancel POS invoices', 3, TRUE
    FROM modules m
    WHERE m.modulecode = 'POS' AND m.cancelled = FALSE
      AND NOT EXISTS (
          SELECT 1 FROM permissions p
          WHERE p.permissioncode = 'pos.cancel' AND p.cancelled = FALSE
      );

    INSERT INTO roles (rolecode, rolename, description, issystem, isactive)
    SELECT v.rolecode, v.rolename, v.description, v.issystem, TRUE
    FROM (VALUES
        ('ADMIN', 'Admin', 'Company administrator. Uses every module the software provider has enabled, and manages employees and roles.', TRUE),
        ('STORE_MANAGER', 'Store Manager', 'Manage a store', TRUE),
        ('CASHIER', 'Cashier', 'POS billing', TRUE),
        ('INVENTORY', 'Inventory Clerk', 'Stock and purchasing', TRUE),
        ('VIEWER', 'Viewer', 'Read-only reporting', TRUE),
        ('SENIOR_CASHIER', 'Senior Cashier', 'POS billing, returns and invoice cancel. Demo role.', FALSE),
        ('PURCHASE_EXEC', 'Purchase Executive', 'Purchasing. Demo role.', FALSE),
        ('SALES_EXEC', 'Sales Executive', 'POS selling and customer view. Demo role.', FALSE),
        ('STOCK_ASSIST', 'Stock Assistant', 'View stock and products. Demo role.', FALSE)
    ) AS v(rolecode, rolename, description, issystem)
    WHERE NOT EXISTS (
        SELECT 1 FROM roles r WHERE r.rolecode = v.rolecode AND r.cancelled = FALSE
    );

    INSERT INTO role_permissions (fk_role, fk_permission)
    SELECT r.id_role, p.id_permission
    FROM (VALUES
        ('ADMIN', 'dashboard.view'),
        ('ADMIN', 'products.view'), ('ADMIN', 'products.manage'),
        ('ADMIN', 'inventory.view'), ('ADMIN', 'inventory.adjust'),
        ('ADMIN', 'purchases.view'), ('ADMIN', 'purchases.manage'),
        ('ADMIN', 'pos.sell'), ('ADMIN', 'pos.return'), ('ADMIN', 'pos.cancel'),
        ('ADMIN', 'customers.view'), ('ADMIN', 'customers.manage'),
        ('ADMIN', 'reports.view'), ('ADMIN', 'settings.manage'), ('ADMIN', 'users.manage'),
        ('STORE_MANAGER', 'dashboard.view'),
        ('STORE_MANAGER', 'products.view'), ('STORE_MANAGER', 'products.manage'),
        ('STORE_MANAGER', 'inventory.view'), ('STORE_MANAGER', 'inventory.adjust'),
        ('STORE_MANAGER', 'purchases.view'), ('STORE_MANAGER', 'purchases.manage'),
        ('STORE_MANAGER', 'pos.sell'), ('STORE_MANAGER', 'pos.return'), ('STORE_MANAGER', 'pos.cancel'),
        ('STORE_MANAGER', 'customers.view'), ('STORE_MANAGER', 'customers.manage'),
        ('STORE_MANAGER', 'reports.view'),
        ('CASHIER', 'pos.sell'),
        ('SENIOR_CASHIER', 'pos.sell'), ('SENIOR_CASHIER', 'pos.return'), ('SENIOR_CASHIER', 'pos.cancel'),
        ('SENIOR_CASHIER', 'products.view'),
        ('INVENTORY', 'inventory.view'), ('INVENTORY', 'inventory.adjust'),
        ('INVENTORY', 'products.view'), ('INVENTORY', 'purchases.view'),
        ('PURCHASE_EXEC', 'purchases.view'), ('PURCHASE_EXEC', 'purchases.manage'),
        ('PURCHASE_EXEC', 'products.view'), ('PURCHASE_EXEC', 'inventory.view'),
        ('SALES_EXEC', 'pos.sell'), ('SALES_EXEC', 'customers.view'),
        ('SALES_EXEC', 'products.view'), ('SALES_EXEC', 'dashboard.view'),
        ('STOCK_ASSIST', 'inventory.view'), ('STOCK_ASSIST', 'products.view'),
        ('VIEWER', 'reports.view'), ('VIEWER', 'dashboard.view'),
        ('VIEWER', 'products.view'), ('VIEWER', 'inventory.view')
    ) AS g(rolecode, permissioncode)
    JOIN roles r ON r.rolecode = g.rolecode AND r.cancelled = FALSE
    JOIN permissions p ON p.permissioncode = g.permissioncode AND p.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM role_permissions rp
        WHERE rp.fk_role = r.id_role AND rp.fk_permission = p.id_permission
    );

    -- Turns on modules this demo uses. Does not remove modules already on.
    INSERT INTO installation_modules (fk_module)
    SELECT m.id_module
    FROM modules m
    WHERE m.cancelled = FALSE
      AND m.isactive = TRUE
      AND NOT EXISTS (
          SELECT 1 FROM installation_modules i WHERE i.fk_module = m.id_module
      );

    INSERT INTO company (
        companycode, companyname, legalname, email, phone, website,
        addressline1, city, state, postalcode, country,
        currencycode, timezone, locale, isactive
    )
    SELECT
        'GREENBASKET', 'Green Basket Supermarket', 'Green Basket Supermarket',
        'hello@greenbasket.example', '0484-0001000', 'https://greenbasket.example',
        'MG Road', 'Kochi', 'Kerala', '682016', 'India',
        'INR', 'Asia/Kolkata', 'en-IN', TRUE
    WHERE NOT EXISTS (
        SELECT 1 FROM company c
        WHERE c.companycode = 'GREENBASKET' AND c.cancelled = FALSE
    );

    SELECT c.id_company INTO v_company
    FROM company c
    WHERE c.companycode = 'GREENBASKET' AND c.cancelled = FALSE
    LIMIT 1;

    IF v_company IS NULL THEN
        RAISE EXCEPTION 'Company GREENBASKET could not be resolved.';
    END IF;

    INSERT INTO store (
        fk_company, storecode, storename, email, phone,
        addressline1, city, state, postalcode, country,
        isdefault, isactive
    )
    SELECT
        v_company, 'GB-KOCHI', 'Green Basket Kochi',
        'kochi@greenbasket.example', '0484-0001001',
        'MG Road', 'Kochi', 'Kerala', '682016', 'India',
        NOT EXISTS (
            SELECT 1 FROM store s
            WHERE s.fk_company = v_company AND s.isdefault = TRUE AND s.cancelled = FALSE
        ),
        TRUE
    WHERE NOT EXISTS (
        SELECT 1 FROM store s
        WHERE s.fk_company = v_company AND s.storecode = 'GB-KOCHI' AND s.cancelled = FALSE
    );

    SELECT s.id_store INTO v_store
    FROM store s
    WHERE s.fk_company = v_company AND s.storecode = 'GB-KOCHI' AND s.cancelled = FALSE
    LIMIT 1;

    IF v_store IS NULL THEN
        RAISE EXCEPTION 'Store GB-KOCHI could not be resolved.';
    END IF;

    INSERT INTO license (licensekey, license_type, status, startdate, expirydate, maxstores, notes)
    SELECT
        'GB-DEMO-TRIAL-20261021', 'trial', 'trial',
        DATE '2026-10-01', DATE '2027-10-01', 1,
        'Demo trial for Green Basket. Test data only. One store, so transfers are not seeded.'
    WHERE NOT EXISTS (SELECT 1 FROM license WHERE cancelled = FALSE);

    INSERT INTO installation_config (
        fk_license, theme, store_mode, onboarding_step, onboarding_complete, activated_at
    )
    SELECT l.id_license, 'light', 'single', 0, TRUE, TIMESTAMP '2026-10-01 09:00:00'
    FROM license l
    WHERE l.cancelled = FALSE
      AND NOT EXISTS (SELECT 1 FROM installation_config)
    ORDER BY l.id_license
    LIMIT 1;

    INSERT INTO units (unitcode, unitname, isactive)
    SELECT v.unitcode, v.unitname, TRUE
    FROM (VALUES
        ('PCS', 'Piece'),
        ('PKT', 'Packet'),
        ('KG', 'Kilogram'),
        ('G', 'Gram'),
        ('LTR', 'Litre'),
        ('ML', 'Millilitre'),
        ('BTL', 'Bottle'),
        ('BOX', 'Box')
    ) AS v(unitcode, unitname)
    WHERE NOT EXISTS (
        SELECT 1 FROM units u WHERE u.unitcode = v.unitcode AND u.cancelled = FALSE
    );

    INSERT INTO taxes (taxcode, taxname, taxrate, isinclusive, isactive)
    SELECT v.taxcode, v.taxname, v.taxrate, FALSE, TRUE
    FROM (VALUES
        ('GST0',  'GST 0% (demo)',  0.0000),
        ('GST5',  'GST 5% (demo)',  5.0000),
        ('GST12', 'GST 12% (demo)', 12.0000),
        ('GST18', 'GST 18% (demo)', 18.0000)
    ) AS v(taxcode, taxname, taxrate)
    WHERE NOT EXISTS (
        SELECT 1 FROM taxes t WHERE t.taxcode = v.taxcode AND t.cancelled = FALSE
    );

    INSERT INTO categories (categorycode, categoryname, description, displayorder, isactive)
    SELECT v.categorycode, v.categoryname, 'Green Basket demo category.', v.displayorder, TRUE
    FROM (VALUES
        ('GB-RICE', 'Rice & Grains', 10),
        ('GB-COOK', 'Cooking Essentials', 20),
        ('GB-DAIRY', 'Dairy & Chilled', 30),
        ('GB-BEV', 'Beverages', 40),
        ('GB-SNACK', 'Snacks & Packaged Foods', 50),
        ('GB-CLEAN', 'Cleaning & Household', 60),
        ('GB-CARE', 'Personal Care', 70),
        ('GB-SHOE', 'Footwear', 80)
    ) AS v(categorycode, categoryname, displayorder)
    WHERE NOT EXISTS (
        SELECT 1 FROM categories c WHERE c.categoryname = v.categoryname AND c.cancelled = FALSE
    );

    INSERT INTO subcategories (fk_category, subcategoryname, description, displayorder, isactive)
    SELECT c.id_category, v.subcategoryname, 'Green Basket demo subcategory.', v.displayorder, TRUE
    FROM (VALUES
        ('Rice & Grains', 'Rice', 10),
        ('Rice & Grains', 'Flours & Grains', 20),
        ('Rice & Grains', 'Pulses', 30),
        ('Cooking Essentials', 'Oils', 10),
        ('Cooking Essentials', 'Staples', 20),
        ('Cooking Essentials', 'Spices', 30),
        ('Dairy & Chilled', 'Milk & Curd', 10),
        ('Dairy & Chilled', 'Butter & Paneer', 20),
        ('Beverages', 'Hot Drinks', 10),
        ('Beverages', 'Cold Drinks', 20),
        ('Snacks & Packaged Foods', 'Snacks', 10),
        ('Snacks & Packaged Foods', 'Breakfast', 20),
        ('Cleaning & Household', 'Laundry', 10),
        ('Cleaning & Household', 'Surface Cleaners', 20),
        ('Personal Care', 'Bath', 10),
        ('Personal Care', 'Oral Care', 20),
        ('Footwear', 'Casual Shoes', 10)
    ) AS v(categoryname, subcategoryname, displayorder)
    JOIN categories c ON c.categoryname = v.categoryname AND c.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM subcategories s
        WHERE s.fk_category = c.id_category
          AND s.subcategoryname = v.subcategoryname
          AND s.cancelled = FALSE
    );

    INSERT INTO brands (brandname, description, isactive)
    SELECT v.brandname, 'Demo catalog name only. Not a claim of authorisation.', TRUE
    FROM (VALUES
        ('India Gate'), ('Fortune'), ('Parachute'), ('Tata'), ('Aashirvaad'),
        ('Nescafe'), ('Brooke Bond'), ('Surf Excel'), ('Vim'), ('Dove'), ('Colgate'),
        ('Kochi Dairy Demo'), ('Green Basket House'), ('Walkwell Demo')
    ) AS v(brandname)
    WHERE NOT EXISTS (
        SELECT 1 FROM brands b WHERE b.brandname = v.brandname AND b.cancelled = FALSE
    );

    INSERT INTO suppliers (
        suppliercode, suppliername, contactperson, email, phone,
        addressline1, city, state, postalcode, country, isactive
    )
    SELECT v.suppliercode, v.suppliername, v.contactperson, v.email, v.phone,
           v.addressline1, 'Kochi', 'Kerala', '682001', 'India', TRUE
    FROM (VALUES
        ('SUP-MBW', 'Malabar Wholesale Distributors', 'Desk MBW', 'desk@malabar-wholesale.example', '0484-0002001', 'Willingdon Island'),
        ('SUP-KGS', 'Kochi Grocery Supplies', 'Desk KGS', 'desk@kochi-grocery.example', '0484-0002002', 'Edappally'),
        ('SUP-KFT', 'Kerala FMCG Traders', 'Desk KFT', 'desk@kerala-fmcg.example', '0484-0002003', 'Kaloor'),
        ('SUP-GLC', 'Greenline Consumer Products', 'Desk GLC', 'desk@greenline-consumer.example', '0484-0002004', 'Palarivattom'),
        ('SUP-MPF', 'Metro Packaged Foods Supply', 'Desk MPF', 'desk@metro-packaged.example', '0484-0002005', 'Vyttila')
    ) AS v(suppliercode, suppliername, contactperson, email, phone, addressline1)
    WHERE NOT EXISTS (
        SELECT 1 FROM suppliers s WHERE s.suppliername = v.suppliername AND s.cancelled = FALSE
    );

    CREATE TEMP TABLE gb_user (
        username TEXT PRIMARY KEY,
        email TEXT NOT NULL,
        fullname TEXT NOT NULL,
        phone TEXT NOT NULL,
        employeecode TEXT NOT NULL,
        rolecode TEXT NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO gb_user (username, email, fullname, phone, employeecode, rolecode) VALUES
        ('gb.admin', 'gb.admin@greenbasket.example', 'Devika Menon', '9000000201', 'EMP-GB-001', 'ADMIN'),
        ('gb.manager', 'gb.manager@greenbasket.example', 'Arun Krishnan', '9000000202', 'EMP-GB-002', 'STORE_MANAGER'),
        ('gb.cashier.senior', 'gb.cashier.senior@greenbasket.example', 'Sreeja Pillai', '9000000203', 'EMP-GB-003', 'SENIOR_CASHIER'),
        ('gb.cashier.1', 'gb.cashier.1@greenbasket.example', 'Nikhil Thomas', '9000000204', 'EMP-GB-004', 'CASHIER'),
        ('gb.cashier.2', 'gb.cashier.2@greenbasket.example', 'Fathima Noor', '9000000205', 'EMP-GB-005', 'CASHIER'),
        ('gb.inventory', 'gb.inventory@greenbasket.example', 'Joseph Mathew', '9000000206', 'EMP-GB-006', 'INVENTORY'),
        ('gb.purchase', 'gb.purchase@greenbasket.example', 'Rahul Varma', '9000000207', 'EMP-GB-007', 'PURCHASE_EXEC'),
        ('gb.sales', 'gb.sales@greenbasket.example', 'Meera Iyer', '9000000208', 'EMP-GB-008', 'SALES_EXEC'),
        ('gb.stock', 'gb.stock@greenbasket.example', 'Anand Das', '9000000209', 'EMP-GB-009', 'STOCK_ASSIST'),
        ('gb.reports', 'gb.reports@greenbasket.example', 'Priya Nambiar', '9000000210', 'EMP-GB-010', 'VIEWER');

    INSERT INTO users (
        username, email, passwordhash, fullname, phone, employeecode,
        isactive, mustchangepassword, is_platform_super_admin
    )
    SELECT u.username, u.email, v_hash, u.fullname, u.phone, u.employeecode,
           TRUE, TRUE, FALSE
    FROM gb_user u
    WHERE NOT EXISTS (
        SELECT 1 FROM users x WHERE x.username = u.username AND x.cancelled = FALSE
    );

    INSERT INTO user_roles (fk_user, fk_role)
    SELECT usr.id_user, r.id_role
    FROM gb_user g
    JOIN users usr ON usr.username = g.username AND usr.cancelled = FALSE
    JOIN roles r ON r.rolecode = g.rolecode AND r.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM user_roles ur
        WHERE ur.fk_user = usr.id_user AND ur.fk_role = r.id_role
    );

    INSERT INTO user_store_access (fk_user, fk_store, isdefault)
    SELECT usr.id_user, v_store, TRUE
    FROM gb_user g
    JOIN users usr ON usr.username = g.username AND usr.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM user_store_access a
        WHERE a.fk_user = usr.id_user AND a.cancelled = FALSE AND a.isdefault = TRUE
    )
    AND NOT EXISTS (
        SELECT 1 FROM user_store_access a
        WHERE a.fk_user = usr.id_user AND a.fk_store = v_store
    );

    SELECT id_user INTO v_admin FROM users WHERE username = 'gb.admin' AND cancelled = FALSE;

    CREATE TEMP TABLE gb_customer (
        customercode TEXT PRIMARY KEY,
        fullname TEXT NOT NULL,
        email TEXT NOT NULL,
        phone TEXT NOT NULL,
        city TEXT NOT NULL,
        addressline1 TEXT NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO gb_customer (customercode, fullname, email, phone, city, addressline1) VALUES
        ('CUS-01', 'Ananya Nair', 'ananya.nair.demo@example.com', '9000000101', 'Kochi', 'Panampilly Nagar'),
        ('CUS-02', 'Rahul Menon', 'rahul.menon.demo@example.com', '9000000102', 'Kochi', 'Kadavanthra'),
        ('CUS-03', 'Meera Joseph', 'meera.joseph.demo@example.com', '9000000103', 'Kochi', 'Fort Kochi'),
        ('CUS-04', 'Joseph Kurian', 'joseph.kurian.demo@example.com', '9000000104', 'Ernakulam', 'Kacheripady'),
        ('CUS-05', 'Fatima Zahra', 'fatima.zahra.demo@example.com', '9000000105', 'Kochi', 'Mattancherry'),
        ('CUS-06', 'Anjali Rao', 'anjali.rao.demo@example.com', '9000000106', 'Kochi', 'Vyttila'),
        ('CUS-07', 'Vivek Shenoy', 'vivek.shenoy.demo@example.com', '9000000107', 'Kochi', 'Edappally'),
        ('CUS-08', 'Suresh Babu', 'suresh.babu.demo@example.com', '9000000108', 'Kochi', 'Kaloor'),
        ('CUS-09', 'Lakshmi Pillai', 'lakshmi.pillai.demo@example.com', '9000000109', 'Kochi', 'Tripunithura'),
        ('CUS-10', 'Imran Kaif', 'imran.kaif.demo@example.com', '9000000110', 'Kochi', 'Aluva'),
        ('CUS-11', 'Diya Krishnan', 'diya.krishnan.demo@example.com', '9000000111', 'Kochi', 'Palarivattom'),
        ('CUS-12', 'Hari Prasad', 'hari.prasad.demo@example.com', '9000000112', 'Kochi', 'Thevara'),
        ('CUS-13', 'Neha Samuel', 'neha.samuel.demo@example.com', '9000000113', 'Kochi', 'Marine Drive'),
        ('CUS-14', 'Abdul Kareem', 'abdul.kareem.demo@example.com', '9000000114', 'Kochi', 'Willingdon Island'),
        ('CUS-15', 'Riya George', 'riya.george.demo@example.com', '9000000115', 'Kochi', 'Kakkanad');

    INSERT INTO customers (customercode, fullname, email, phone, isactive)
    SELECT c.customercode, c.fullname, c.email, c.phone, TRUE
    FROM gb_customer c
    WHERE NOT EXISTS (
        SELECT 1 FROM customers x WHERE x.customercode = c.customercode AND x.cancelled = FALSE
    );

    INSERT INTO customer_addresses (
        fk_customer, addresstype, addressline1, city, state, postalcode, country, isdefault
    )
    SELECT cu.id_customer, 'shipping', g.addressline1, g.city, 'Kerala', '682016', 'India', TRUE
    FROM gb_customer g
    JOIN customers cu ON cu.customercode = g.customercode AND cu.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM customer_addresses a
        WHERE a.fk_customer = cu.id_customer AND a.cancelled = FALSE
    );

    CREATE TEMP TABLE gb_product (
        code TEXT PRIMARY KEY,
        productname TEXT NOT NULL,
        categoryname TEXT NOT NULL,
        subcategoryname TEXT NOT NULL,
        brandname TEXT NOT NULL,
        unitcode TEXT NOT NULL,
        taxcode TEXT NOT NULL,
        costprice NUMERIC(12,2) NOT NULL,
        sellingprice NUMERIC(12,2) NOT NULL,
        mrp NUMERIC(12,2) NOT NULL,
        isonline BOOLEAN NOT NULL,
        reorderlevel NUMERIC(14,3) NOT NULL,
        sku TEXT,
        barcode TEXT,
        description TEXT NOT NULL,
        id_product INT
    ) ON COMMIT DROP;

    INSERT INTO gb_product (
        code, productname, categoryname, subcategoryname, brandname, unitcode, taxcode,
        costprice, sellingprice, mrp, isonline, reorderlevel, sku, barcode, description
    ) VALUES
        ('GB-RICE-IG', 'India Gate Basmati Rice', 'Rice & Grains', 'Rice', 'India Gate', 'PKT', 'GST5', 74, 92, 110, TRUE, 20, NULL, NULL, 'Demo parent. Sell the 1 kg and 5 kg SKUs, not this parent.'),
        ('GB-RICE-MATTA', 'Kerala Matta Rice', 'Rice & Grains', 'Rice', 'Green Basket House', 'PKT', 'GST5', 40, 52, 60, TRUE, 15, NULL, NULL, 'Demo parent for 1 kg and 5 kg matta packs.'),
        ('GB-RICE-PONNI', 'Ponni Boiled Rice 5kg', 'Rice & Grains', 'Rice', 'Green Basket House', 'PKT', 'GST5', 210, 265, 290, TRUE, 10, 'GB-SKU-PONNI-5KG', 'GB-BC-PONNI-5KG', 'Standalone 5 kg pack.'),
        ('GB-RICE-BROKEN', 'Broken Rice 1kg', 'Rice & Grains', 'Rice', 'Green Basket House', 'PKT', 'GST5', 36, 48, 55, TRUE, 12, 'GB-SKU-BROKEN-1KG', 'GB-BC-BROKEN-1KG', 'Standalone 1 kg pack.'),
        ('GB-FLOUR-WHEAT', 'Wheat Flour 1kg', 'Rice & Grains', 'Flours & Grains', 'Green Basket House', 'PKT', 'GST5', 32, 44, 50, TRUE, 12, 'GB-SKU-WHEAT-1KG', 'GB-BC-WHEAT-1KG', 'Standalone wheat flour.'),
        ('GB-ATTA', 'Aashirvaad Atta', 'Rice & Grains', 'Flours & Grains', 'Aashirvaad', 'PKT', 'GST5', 46, 58, 68, TRUE, 15, NULL, NULL, 'Demo parent for 1 kg and 5 kg atta.'),
        ('GB-MAIDA', 'Maida 1kg', 'Rice & Grains', 'Flours & Grains', 'Green Basket House', 'PKT', 'GST5', 34, 46, 52, TRUE, 10, 'GB-SKU-MAIDA-1KG', 'GB-BC-MAIDA-1KG', 'Standalone maida.'),
        ('GB-RAVA', 'Rava 500g', 'Rice & Grains', 'Flours & Grains', 'Green Basket House', 'PKT', 'GST5', 28, 38, 44, TRUE, 10, 'GB-SKU-RAVA-500G', 'GB-BC-RAVA-500G', 'Standalone rava.'),
        ('GB-DAL-TOOR', 'Toor Dal 1kg', 'Rice & Grains', 'Pulses', 'Tata', 'PKT', 'GST5', 110, 142, 160, TRUE, 12, 'GB-SKU-TOOR-1KG', 'GB-BC-TOOR-1KG', 'Standalone toor dal.'),
        ('GB-DAL-MOONG', 'Moong Dal 500g', 'Rice & Grains', 'Pulses', 'Tata', 'PKT', 'GST5', 62, 82, 95, TRUE, 10, 'GB-SKU-MOONG-500G', 'GB-BC-MOONG-500G', 'Standalone moong dal.'),
        ('GB-POHA', 'Poha 500g', 'Rice & Grains', 'Flours & Grains', 'Green Basket House', 'PKT', 'GST5', 24, 34, 40, TRUE, 10, 'GB-SKU-POHA-500G', 'GB-BC-POHA-500G', 'Standalone poha.'),
        ('GB-OIL-SUN', 'Fortune Sunflower Oil', 'Cooking Essentials', 'Oils', 'Fortune', 'BTL', 'GST5', 128, 149, 165, TRUE, 12, NULL, NULL, 'Demo parent for 1 L and 5 L.'),
        ('GB-OIL-COCO', 'Parachute Coconut Oil', 'Cooking Essentials', 'Oils', 'Parachute', 'BTL', 'GST5', 80, 98, 115, TRUE, 10, NULL, NULL, 'Demo parent for 500 ml and 1 L.'),
        ('GB-OIL-OLIVE', 'Olive Oil 500ml', 'Cooking Essentials', 'Oils', 'Green Basket House', 'BTL', 'GST5', 280, 349, 399, TRUE, 4, 'GB-SKU-OLIVE-500', 'GB-BC-OLIVE-500', 'Seeded down to zero on-hand.'),
        ('GB-SUGAR', 'Sugar 1kg', 'Cooking Essentials', 'Staples', 'Green Basket House', 'PKT', 'GST5', 38, 46, 52, TRUE, 20, 'GB-SKU-SUGAR-1KG', 'GB-BC-SUGAR-1KG', 'Partially received purchase.'),
        ('GB-JAGGERY', 'Jaggery 500g', 'Cooking Essentials', 'Staples', 'Green Basket House', 'PKT', 'GST5', 32, 44, 50, TRUE, 8, 'GB-SKU-JAGGERY-500', 'GB-BC-JAGGERY-500', 'Standalone jaggery.'),
        ('GB-SALT', 'Tata Salt 1kg', 'Cooking Essentials', 'Staples', 'Tata', 'PKT', 'GST0', 18, 24, 28, TRUE, 20, 'GB-SKU-SALT-1KG', 'GB-BC-SALT-1KG', 'Demo GST 0% salt.'),
        ('GB-TURMERIC', 'Turmeric Powder 100g', 'Cooking Essentials', 'Spices', 'Green Basket House', 'PKT', 'GST5', 22, 32, 38, TRUE, 10, 'GB-SKU-TURMERIC-100', 'GB-BC-TURMERIC-100', 'Standalone turmeric.'),
        ('GB-CHILLI', 'Chilli Powder 100g', 'Cooking Essentials', 'Spices', 'Green Basket House', 'PKT', 'GST5', 24, 36, 42, TRUE, 10, 'GB-SKU-CHILLI-100', 'GB-BC-CHILLI-100', 'Standalone chilli powder.'),
        ('GB-MILK', 'Toned Milk', 'Dairy & Chilled', 'Milk & Curd', 'Kochi Dairy Demo', 'BTL', 'GST5', 24, 28, 30, TRUE, 24, NULL, NULL, 'Demo parent for 500 ml and 1 L.'),
        ('GB-BUTTER', 'Butter 100g', 'Dairy & Chilled', 'Butter & Paneer', 'Kochi Dairy Demo', 'PCS', 'GST12', 42, 56, 62, TRUE, 10, 'GB-SKU-BUTTER-100', 'GB-BC-BUTTER-100', 'Standalone butter.'),
        ('GB-CHEESE', 'Cheese Slices 200g', 'Dairy & Chilled', 'Butter & Paneer', 'Kochi Dairy Demo', 'PKT', 'GST12', 78, 105, 120, TRUE, 8, 'GB-SKU-CHEESE-200', 'GB-BC-CHEESE-200', 'Standalone cheese slices.'),
        ('GB-CURD', 'Curd 500g', 'Dairy & Chilled', 'Milk & Curd', 'Kochi Dairy Demo', 'PKT', 'GST5', 28, 38, 42, TRUE, 12, 'GB-SKU-CURD-500', 'GB-BC-CURD-500', 'Standalone curd.'),
        ('GB-PANEER', 'Paneer 200g', 'Dairy & Chilled', 'Butter & Paneer', 'Kochi Dairy Demo', 'PKT', 'GST5', 62, 84, 95, TRUE, 8, 'GB-SKU-PANEER-200', 'GB-BC-PANEER-200', 'Standalone paneer.'),
        ('GB-GHEE', 'Ghee 200ml', 'Dairy & Chilled', 'Butter & Paneer', 'Green Basket House', 'BTL', 'GST12', 145, 189, 210, TRUE, 6, 'GB-SKU-GHEE-200', 'GB-BC-GHEE-200', 'Standalone ghee.'),
        ('GB-TEA', 'Brooke Bond Tea', 'Beverages', 'Hot Drinks', 'Brooke Bond', 'PKT', 'GST5', 98, 125, 140, TRUE, 10, NULL, NULL, 'Demo parent for 250 g and 500 g.'),
        ('GB-COFFEE', 'Coffee Powder 200g', 'Beverages', 'Hot Drinks', 'Green Basket House', 'PKT', 'GST5', 95, 128, 145, TRUE, 8, 'GB-SKU-COFFEE-200', 'GB-BC-COFFEE-200', 'Standalone coffee powder.'),
        ('GB-NESCAFE', 'Nescafe Classic 50g', 'Beverages', 'Hot Drinks', 'Nescafe', 'PCS', 'GST18', 115, 155, 175, TRUE, 8, 'GB-SKU-NESCAFE-50', 'GB-BC-NESCAFE-50', 'Standalone instant coffee.'),
        ('GB-WATER', 'Drinking Water 1L', 'Beverages', 'Cold Drinks', 'Green Basket House', 'BTL', 'GST18', 12, 20, 20, TRUE, 24, 'GB-SKU-WATER-1L', 'GB-BC-WATER-1L', 'Standalone water.'),
        ('GB-JUICE', 'Mixed Fruit Juice 1L', 'Beverages', 'Cold Drinks', 'Green Basket House', 'BTL', 'GST12', 70, 95, 110, TRUE, 8, 'GB-SKU-JUICE-1L', 'GB-BC-JUICE-1L', 'Standalone juice.'),
        ('GB-COLA', 'Soft Drink 750ml', 'Beverages', 'Cold Drinks', 'Green Basket House', 'BTL', 'GST12', 28, 40, 40, TRUE, 12, 'GB-SKU-COLA-750', 'GB-BC-COLA-750', 'Standalone soft drink.'),
        ('GB-CHIPS', 'Potato Chips 52g', 'Snacks & Packaged Foods', 'Snacks', 'Green Basket House', 'PKT', 'GST12', 12, 20, 20, TRUE, 15, 'GB-SKU-CHIPS-52', 'GB-BC-CHIPS-52', 'Standalone chips.'),
        ('GB-BISCUIT', 'Marie Biscuits 250g', 'Snacks & Packaged Foods', 'Snacks', 'Green Basket House', 'PKT', 'GST12', 22, 35, 40, TRUE, 12, 'GB-SKU-BISCUIT-250', 'GB-BC-BISCUIT-250', 'Standalone biscuits.'),
        ('GB-COOKIE', 'Butter Cookies 150g', 'Snacks & Packaged Foods', 'Snacks', 'Green Basket House', 'PKT', 'GST12', 30, 45, 50, TRUE, 8, 'GB-SKU-COOKIE-150', 'GB-BC-COOKIE-150', 'Standalone cookies.'),
        ('GB-NOODLE', 'Instant Noodles 70g', 'Snacks & Packaged Foods', 'Snacks', 'Green Basket House', 'PKT', 'GST12', 10, 15, 15, TRUE, 20, 'GB-SKU-NOODLE-70', 'GB-BC-NOODLE-70', 'Also appears on an unreceived posted purchase.'),
        ('GB-PASTA', 'Pasta 500g', 'Snacks & Packaged Foods', 'Breakfast', 'Green Basket House', 'PKT', 'GST12', 48, 72, 85, TRUE, 8, 'GB-SKU-PASTA-500', 'GB-BC-PASTA-500', 'Standalone pasta.'),
        ('GB-CEREAL', 'Breakfast Cereal 400g', 'Snacks & Packaged Foods', 'Breakfast', 'Green Basket House', 'BOX', 'GST12', 95, 145, 165, TRUE, 6, 'GB-SKU-CEREAL-400', 'GB-BC-CEREAL-400', 'Standalone cereal.'),
        ('GB-CHOCO', 'Milk Chocolate 40g', 'Snacks & Packaged Foods', 'Snacks', 'Green Basket House', 'PCS', 'GST18', 18, 30, 35, TRUE, 12, 'GB-SKU-CHOCO-40', 'GB-BC-CHOCO-40', 'Standalone chocolate.'),
        ('GB-PB', 'Peanut Butter 340g', 'Snacks & Packaged Foods', 'Breakfast', 'Green Basket House', 'PCS', 'GST12', 120, 165, 185, TRUE, 6, 'GB-SKU-PB-340', 'GB-BC-PB-340', 'Standalone peanut butter.'),
        ('GB-WASH', 'Surf Excel Washing Powder', 'Cleaning & Household', 'Laundry', 'Surf Excel', 'PKT', 'GST18', 50, 65, 75, TRUE, 10, NULL, NULL, 'Parent price is the 500 g pack. 1 kg and 2 kg add to it. 500 g SKU is inactive.'),
        ('GB-DISH', 'Vim Dishwash Liquid 500ml', 'Cleaning & Household', 'Surface Cleaners', 'Vim', 'BTL', 'GST18', 55, 79, 89, TRUE, 8, 'GB-SKU-DISH-500', 'GB-BC-DISH-500', 'Standalone dishwash liquid.'),
        ('GB-TOILET', 'Toilet Cleaner 500ml', 'Cleaning & Household', 'Surface Cleaners', 'Green Basket House', 'BTL', 'GST18', 62, 89, 99, TRUE, 6, 'GB-SKU-TOILET-500', 'GB-BC-TOILET-500', 'Standalone toilet cleaner.'),
        ('GB-FLOOR', 'Floor Cleaner 1L', 'Cleaning & Household', 'Surface Cleaners', 'Green Basket House', 'BTL', 'GST18', 85, 115, 130, TRUE, 6, 'GB-SKU-FLOOR-1L', 'GB-BC-FLOOR-1L', 'Standalone floor cleaner.'),
        ('GB-LSOAP', 'Laundry Soap', 'Cleaning & Household', 'Laundry', 'Green Basket House', 'PCS', 'GST18', 18, 28, 32, TRUE, 12, 'GB-SKU-LSOAP', 'GB-BC-LSOAP', 'Standalone laundry soap.'),
        ('GB-SOAP', 'Dove Bath Soap', 'Personal Care', 'Bath', 'Dove', 'PCS', 'GST18', 28, 42, 49, TRUE, 12, NULL, NULL, 'Demo parent for a single bar and a pack of 3.'),
        ('GB-SHAMPOO', 'Shampoo 180ml', 'Personal Care', 'Bath', 'Dove', 'BTL', 'GST18', 95, 135, 155, TRUE, 8, 'GB-SKU-SHAMPOO-180', 'GB-BC-SHAMPOO-180', 'Standalone shampoo.'),
        ('GB-TOOTH', 'Colgate Toothpaste 150g', 'Personal Care', 'Oral Care', 'Colgate', 'PCS', 'GST18', 62, 89, 99, TRUE, 10, 'GB-SKU-TOOTH-150', 'GB-BC-TOOTH-150', 'Standalone toothpaste.'),
        ('GB-SHOE', 'Chelsea Casual Shoes', 'Footwear', 'Casual Shoes', 'Walkwell Demo', 'PCS', 'GST18', 1800, 2499, 2999, FALSE, 2, NULL, NULL, 'Named SKUs only. Tan/Black and size are text, not attribute axes. Not sold online.'),
        ('GB-VERMI', 'Vermicelli 400g', 'Snacks & Packaged Foods', 'Breakfast', 'Green Basket House', 'PKT', 'GST5', 28, 42, 48, TRUE, 8, 'GB-SKU-VERMI-400', 'GB-BC-VERMI-400', 'Standalone vermicelli.'),
        ('GB-BREAD', 'Sandwich Bread 400g', 'Snacks & Packaged Foods', 'Breakfast', 'Green Basket House', 'PKT', 'GST5', 28, 40, 45, TRUE, 10, 'GB-SKU-BREAD-400', 'GB-BC-BREAD-400', 'Standalone bread.'),
        ('GB-BANANA', 'Banana Chips 200g', 'Snacks & Packaged Foods', 'Snacks', 'Green Basket House', 'PKT', 'GST12', 45, 70, 80, TRUE, 8, 'GB-SKU-BANANA-200', 'GB-BC-BANANA-200', 'Standalone banana chips.'),
        ('GB-HONEY', 'Honey 250g', 'Cooking Essentials', 'Staples', 'Green Basket House', 'BTL', 'GST5', 95, 145, 165, TRUE, 6, 'GB-SKU-HONEY-250', 'GB-BC-HONEY-250', 'Also appears on a draft purchase that does not add stock.');

    IF (SELECT COUNT(*) FROM gb_product) < 50 THEN
        RAISE EXCEPTION 'Demo catalog has fewer than 50 products.';
    END IF;

    INSERT INTO products (
        fk_category, fk_subcategory, fk_brand, fk_unit, fk_tax,
        productcode, productname, description, sku, barcode,
        costprice, sellingprice, mrp, reorderlevel, isactive, isonline
    )
    SELECT
        c.id_category, sc.id_subcategory, b.id_brand, u.id_unit, t.id_tax,
        p.code, p.productname, p.description, p.sku, p.barcode,
        p.costprice, p.sellingprice, p.mrp, p.reorderlevel, TRUE, p.isonline
    FROM gb_product p
    JOIN categories c ON c.categoryname = p.categoryname AND c.cancelled = FALSE
    JOIN subcategories sc ON sc.fk_category = c.id_category AND sc.subcategoryname = p.subcategoryname AND sc.cancelled = FALSE
    JOIN brands b ON b.brandname = p.brandname AND b.cancelled = FALSE
    JOIN units u ON u.unitcode = p.unitcode AND u.cancelled = FALSE
    JOIN taxes t ON t.taxcode = p.taxcode AND t.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM products x WHERE x.productcode = p.code
    );

    UPDATE gb_product g
    SET id_product = p.id_product
    FROM products p
    WHERE p.productcode = g.code;

    IF EXISTS (SELECT 1 FROM gb_product WHERE id_product IS NULL) THEN
        RAISE EXCEPTION 'A demo product did not resolve to products.id_product.';
    END IF;

    CREATE TEMP TABLE gb_sell (
        item_key TEXT PRIMARY KEY,
        product_code TEXT NOT NULL,
        variant_name TEXT,
        sku TEXT NOT NULL,
        barcode TEXT NOT NULL,
        add_price NUMERIC(12,2) NOT NULL DEFAULT 0,
        unit_cost NUMERIC(12,2) NOT NULL,
        po_code TEXT NOT NULL,
        order_qty NUMERIC(14,3) NOT NULL DEFAULT 80,
        receive_qty NUMERIC(14,3) NOT NULL DEFAULT 80,
        reorder_level NUMERIC(14,3) NOT NULL DEFAULT 12,
        is_active BOOLEAN NOT NULL DEFAULT TRUE,
        id_product INT,
        id_variant INT
    ) ON COMMIT DROP;

    INSERT INTO gb_sell (item_key, product_code, variant_name, sku, barcode, add_price, unit_cost, po_code, reorder_level, is_active)
    SELECT p.sku, p.code, NULL, p.sku, p.barcode, 0, p.costprice,
           CASE p.categoryname
               WHEN 'Rice & Grains' THEN CASE WHEN p.code IN ('GB-FLOUR-WHEAT','GB-MAIDA','GB-RAVA') THEN 'PO-RCV-06' ELSE 'PO-RCV-01' END
               WHEN 'Cooking Essentials' THEN 'PO-RCV-02'
               WHEN 'Dairy & Chilled' THEN 'PO-RCV-03'
               WHEN 'Beverages' THEN 'PO-RCV-03'
               WHEN 'Snacks & Packaged Foods' THEN CASE WHEN p.code IN ('GB-VERMI','GB-BREAD','GB-BANANA') THEN 'PO-RCV-07' ELSE 'PO-RCV-04' END
               WHEN 'Cleaning & Household' THEN 'PO-RCV-05'
               WHEN 'Personal Care' THEN 'PO-RCV-05'
               ELSE 'PO-RCV-07'
           END,
           p.reorderlevel,
           TRUE
    FROM gb_product p
    WHERE p.sku IS NOT NULL;

    INSERT INTO gb_sell (item_key, product_code, variant_name, sku, barcode, add_price, unit_cost, po_code, reorder_level, is_active) VALUES
        ('GB-SKU-RICE-IG-1KG', 'GB-RICE-IG', '1 kg', 'GB-SKU-RICE-IG-1KG', 'GB-BC-RICE-IG-1KG', 0, 74, 'PO-RCV-01', 20, TRUE),
        ('GB-SKU-RICE-IG-5KG', 'GB-RICE-IG', '5 kg', 'GB-SKU-RICE-IG-5KG', 'GB-BC-RICE-IG-5KG', 368, 340, 'PO-RCV-01', 8, TRUE),
        ('GB-SKU-MATTA-1KG', 'GB-RICE-MATTA', '1 kg', 'GB-SKU-MATTA-1KG', 'GB-BC-MATTA-1KG', 0, 40, 'PO-RCV-01', 15, TRUE),
        ('GB-SKU-MATTA-5KG', 'GB-RICE-MATTA', '5 kg', 'GB-SKU-MATTA-5KG', 'GB-BC-MATTA-5KG', 168, 190, 'PO-RCV-01', 8, TRUE),
        ('GB-SKU-ATTA-1KG', 'GB-ATTA', '1 kg', 'GB-SKU-ATTA-1KG', 'GB-BC-ATTA-1KG', 0, 46, 'PO-RCV-06', 15, TRUE),
        ('GB-SKU-ATTA-5KG', 'GB-ATTA', '5 kg', 'GB-SKU-ATTA-5KG', 'GB-BC-ATTA-5KG', 192, 210, 'PO-RCV-06', 8, TRUE),
        ('GB-SKU-SUN-1L', 'GB-OIL-SUN', '1 L', 'GB-SKU-SUN-1L', 'GB-BC-SUN-1L', 0, 128, 'PO-RCV-02', 12, TRUE),
        ('GB-SKU-SUN-5L', 'GB-OIL-SUN', '5 L', 'GB-SKU-SUN-5L', 'GB-BC-SUN-5L', 546, 590, 'PO-RCV-02', 6, TRUE),
        ('GB-SKU-COCO-500', 'GB-OIL-COCO', '500 ml', 'GB-SKU-COCO-500', 'GB-BC-COCO-500', 0, 80, 'PO-RCV-02', 10, TRUE),
        ('GB-SKU-COCO-1L', 'GB-OIL-COCO', '1 L', 'GB-SKU-COCO-1L', 'GB-BC-COCO-1L', 82, 150, 'PO-RCV-02', 8, TRUE),
        ('GB-SKU-MILK-500', 'GB-MILK', '500 ml', 'GB-SKU-MILK-500', 'GB-BC-MILK-500', 0, 24, 'PO-RCV-03', 24, TRUE),
        ('GB-SKU-MILK-1L', 'GB-MILK', '1 L', 'GB-SKU-MILK-1L', 'GB-BC-MILK-1L', 26, 46, 'PO-RCV-03', 24, TRUE),
        ('GB-SKU-TEA-250', 'GB-TEA', '250 g', 'GB-SKU-TEA-250', 'GB-BC-TEA-250', 0, 98, 'PO-RCV-03', 10, TRUE),
        ('GB-SKU-TEA-500', 'GB-TEA', '500 g', 'GB-SKU-TEA-500', 'GB-BC-TEA-500', 105, 185, 'PO-RCV-03', 8, TRUE),
        ('GB-SKU-WASH-500', 'GB-WASH', '500 g', 'GB-SKU-WASH-500', 'GB-BC-WASH-500', 0, 50, 'PO-RCV-05', 0, FALSE),
        ('GB-SKU-WASH-1KG', 'GB-WASH', '1 kg', 'GB-SKU-WASH-1KG', 'GB-BC-WASH-1KG', 53, 92, 'PO-RCV-05', 10, TRUE),
        ('GB-SKU-WASH-2KG', 'GB-WASH', '2 kg', 'GB-SKU-WASH-2KG', 'GB-BC-WASH-2KG', 150, 160, 'PO-RCV-05', 15, TRUE),
        ('GB-SKU-SOAP-1', 'GB-SOAP', 'Single bar', 'GB-SKU-SOAP-1', 'GB-BC-SOAP-1', 0, 28, 'PO-RCV-05', 12, TRUE),
        ('GB-SKU-SOAP-3', 'GB-SOAP', 'Pack of 3', 'GB-SKU-SOAP-3', 'GB-BC-SOAP-3', 68, 80, 'PO-RCV-05', 8, TRUE),
        ('GB-SKU-SHOE-TN-8', 'GB-SHOE', 'Tan / Size 8', 'GB-SKU-SHOE-TN-8', 'GB-BC-SHOE-TN-8', 0, 1800, 'PO-RCV-05', 2, TRUE),
        ('GB-SKU-SHOE-TN-9', 'GB-SHOE', 'Tan / Size 9', 'GB-SKU-SHOE-TN-9', 'GB-BC-SHOE-TN-9', 0, 1800, 'PO-RCV-05', 2, TRUE),
        ('GB-SKU-SHOE-BK-8', 'GB-SHOE', 'Black / Size 8', 'GB-SKU-SHOE-BK-8', 'GB-BC-SHOE-BK-8', 200, 1900, 'PO-RCV-05', 2, TRUE),
        ('GB-SKU-SHOE-BK-9', 'GB-SHOE', 'Black / Size 9', 'GB-SKU-SHOE-BK-9', 'GB-BC-SHOE-BK-9', 200, 1900, 'PO-RCV-05', 2, TRUE);

    UPDATE gb_sell SET order_qty = 20, receive_qty = 10 WHERE item_key = 'GB-SKU-SUGAR-1KG';
    UPDATE gb_sell SET order_qty = 4, receive_qty = 4 WHERE item_key = 'GB-SKU-OLIVE-500';
    UPDATE gb_sell SET order_qty = 3, receive_qty = 3 WHERE item_key = 'GB-SKU-SHOE-BK-9';
    UPDATE gb_sell SET order_qty = 20, receive_qty = 20, reorder_level = 15 WHERE item_key = 'GB-SKU-WASH-2KG';
    UPDATE gb_sell SET order_qty = 0, receive_qty = 0 WHERE item_key = 'GB-SKU-WASH-500';

    UPDATE gb_sell s
    SET id_product = p.id_product
    FROM gb_product p
    WHERE p.code = s.product_code;

    INSERT INTO product_variants (
        fk_product, variantname, skucode, barcode, additionalprice, isactive
    )
    SELECT s.id_product, s.variant_name, s.sku, s.barcode, s.add_price, s.is_active
    FROM gb_sell s
    WHERE s.variant_name IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM product_variants v
          WHERE v.fk_product = s.id_product AND v.variantname = s.variant_name
      );

    UPDATE gb_sell s
    SET id_variant = v.id_productvariant
    FROM product_variants v
    WHERE s.variant_name IS NOT NULL
      AND v.fk_product = s.id_product
      AND v.variantname = s.variant_name;

    IF EXISTS (
        SELECT 1 FROM gb_sell
        WHERE variant_name IS NOT NULL AND id_variant IS NULL
    ) THEN
        RAISE EXCEPTION 'A demo variant did not resolve.';
    END IF;

    INSERT INTO product_images (fk_product, imagepath, displayorder, isprimary)
    SELECT p.id_product, 'demo/greenbasket/' || p.code || '.svg', 0, TRUE
    FROM gb_product p
    WHERE NOT EXISTS (
        SELECT 1 FROM product_images i
        WHERE i.fk_product = p.id_product AND i.cancelled = FALSE
    );

    CREATE TEMP TABLE gb_po (
        po_code TEXT PRIMARY KEY,
        supplier_code TEXT NOT NULL,
        status TEXT NOT NULL,
        invoice_no TEXT NOT NULL,
        po_date DATE NOT NULL,
        id_purchase INT
    ) ON COMMIT DROP;

    INSERT INTO gb_po (po_code, supplier_code, status, invoice_no, po_date) VALUES
        ('PO-RCV-01', 'SUP-MBW', 'received', 'GB-DEMO-PO-001', DATE '2026-10-02'),
        ('PO-RCV-02', 'SUP-KGS', 'partially_received', 'GB-DEMO-PO-002', DATE '2026-10-03'),
        ('PO-RCV-03', 'SUP-KFT', 'received', 'GB-DEMO-PO-003', DATE '2026-10-04'),
        ('PO-RCV-04', 'SUP-GLC', 'received', 'GB-DEMO-PO-004', DATE '2026-10-05'),
        ('PO-RCV-05', 'SUP-MPF', 'received', 'GB-DEMO-PO-005', DATE '2026-10-06'),
        ('PO-RCV-06', 'SUP-MBW', 'received', 'GB-DEMO-PO-006', DATE '2026-10-06'),
        ('PO-RCV-07', 'SUP-MPF', 'received', 'GB-DEMO-PO-007', DATE '2026-10-07'),
        ('PO-POST-08', 'SUP-KGS', 'posted', 'GB-DEMO-PO-008', DATE '2026-10-18'),
        ('PO-DRF-09', 'SUP-GLC', 'draft', 'GB-DEMO-PO-009', DATE '2026-10-19'),
        ('PO-DRF-10', 'SUP-KFT', 'draft', 'GB-DEMO-PO-010', DATE '2026-10-19');

    CREATE TEMP TABLE gb_po_line (
        po_code TEXT NOT NULL,
        item_key TEXT NOT NULL,
        order_qty NUMERIC(14,3) NOT NULL,
        receive_qty NUMERIC(14,3) NOT NULL,
        PRIMARY KEY (po_code, item_key)
    ) ON COMMIT DROP;

    INSERT INTO gb_po_line (po_code, item_key, order_qty, receive_qty)
    SELECT po_code, item_key, order_qty, receive_qty
    FROM gb_sell
    WHERE is_active = TRUE;

    INSERT INTO gb_po_line (po_code, item_key, order_qty, receive_qty) VALUES
        ('PO-POST-08', 'GB-SKU-NOODLE-70', 30, 0),
        ('PO-DRF-09', 'GB-SKU-HONEY-250', 12, 0),
        ('PO-DRF-10', 'GB-SKU-WATER-1L', 24, 0);

    INSERT INTO purchases (
        fk_store, fk_supplier, purchasedate, invoicenumber,
        subtotal, taxamount, discountamount, totalamount,
        notes, status, createdby
    )
    SELECT
        v_store,
        sup.id_supplier,
        h.po_date,
        h.invoice_no,
        SUM(ROUND(l.order_qty * s.unit_cost, 2)),
        SUM(ROUND(l.order_qty * s.unit_cost * t.taxrate / 100, 2)),
        0,
        SUM(ROUND(l.order_qty * s.unit_cost, 2))
            + SUM(ROUND(l.order_qty * s.unit_cost * t.taxrate / 100, 2)),
        'GB-DEMO|purchase ' || h.po_code,
        h.status,
        v_admin
    FROM gb_po h
    JOIN suppliers sup ON sup.suppliercode = h.supplier_code AND sup.cancelled = FALSE
    JOIN gb_po_line l ON l.po_code = h.po_code
    JOIN gb_sell s ON s.item_key = l.item_key
    JOIN gb_product gp ON gp.code = s.product_code
    JOIN taxes t ON t.taxcode = gp.taxcode AND t.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM purchases p
        WHERE p.fk_store = v_store AND p.invoicenumber = h.invoice_no
    )
    GROUP BY sup.id_supplier, h.po_date, h.invoice_no, h.status, h.po_code;

    UPDATE gb_po h
    SET id_purchase = p.id_purchase
    FROM purchases p
    WHERE p.fk_store = v_store AND p.invoicenumber = h.invoice_no;

    IF EXISTS (SELECT 1 FROM gb_po WHERE id_purchase IS NULL) THEN
        RAISE EXCEPTION 'A demo purchase did not resolve.';
    END IF;

    INSERT INTO purchase_details (
        fk_purchase, fk_store, fk_product, fk_productvariant,
        quantity, unitcost, taxamount, linetotal
    )
    SELECT
        h.id_purchase,
        v_store,
        s.id_product,
        s.id_variant,
        l.order_qty,
        s.unit_cost,
        ROUND(l.order_qty * s.unit_cost * t.taxrate / 100, 2),
        ROUND(l.order_qty * s.unit_cost, 2) + ROUND(l.order_qty * s.unit_cost * t.taxrate / 100, 2)
    FROM gb_po_line l
    JOIN gb_po h ON h.po_code = l.po_code
    JOIN gb_sell s ON s.item_key = l.item_key
    JOIN gb_product gp ON gp.code = s.product_code
    JOIN taxes t ON t.taxcode = gp.taxcode AND t.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM purchase_details d
        WHERE d.fk_purchase = h.id_purchase
          AND d.fk_product = s.id_product
          AND d.fk_productvariant IS NOT DISTINCT FROM s.id_variant
    );

    INSERT INTO stock_movements (
        fk_store, fk_product, fk_productvariant, movementtype, quantity,
        referencetype, referenceid, notes, createdby, createdat
    )
    SELECT
        v_store, s.id_product, s.id_variant, 'purchase', l.receive_qty,
        'purchase', h.id_purchase,
        'GB-DEMO|purchase ' || h.invoice_no,
        v_admin,
        h.po_date::timestamp
    FROM gb_po_line l
    JOIN gb_po h ON h.po_code = l.po_code
    JOIN gb_sell s ON s.item_key = l.item_key
    WHERE l.receive_qty > 0
      AND NOT EXISTS (
          SELECT 1 FROM stock_movements m
          WHERE m.referencetype = 'purchase'
            AND m.referenceid = h.id_purchase
            AND m.fk_product = s.id_product
            AND m.fk_productvariant IS NOT DISTINCT FROM s.id_variant
            AND m.movementtype = 'purchase'
      );

    INSERT INTO cash_sessions (
        fk_store, fk_user, openedat, closedat, openingfloat,
        closingcash, expectedcash, variance, status, notes
    )
    SELECT
        v_store,
        (SELECT id_user FROM users WHERE username = 'gb.cashier.senior' AND cancelled = FALSE),
        TIMESTAMP '2026-10-03 09:00:00',
        TIMESTAMP '2026-10-20 21:00:00',
        2000, 2000, 2000, 0, 'closed',
        'GB-DEMO|closed session for seeded invoices'
    WHERE NOT EXISTS (
        SELECT 1 FROM cash_sessions c
        WHERE c.fk_store = v_store AND c.notes = 'GB-DEMO|closed session for seeded invoices'
    );

    SELECT id_cashsession INTO v_session
    FROM cash_sessions
    WHERE fk_store = v_store AND notes = 'GB-DEMO|closed session for seeded invoices'
    LIMIT 1;

    CREATE TEMP TABLE gb_sale (
        invoice TEXT NOT NULL,
        sold_on DATE NOT NULL,
        cashier TEXT NOT NULL,
        customer_code TEXT,
        item_key TEXT NOT NULL,
        qty NUMERIC(14,3) NOT NULL,
        pay_method TEXT NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO gb_sale (invoice, sold_on, cashier, customer_code, item_key, qty, pay_method) VALUES
        ('GB-DEMO-S-001', DATE '2026-10-03', 'gb.cashier.1', 'CUS-01', 'GB-SKU-RICE-IG-1KG', 1, 'cash'),
        ('GB-DEMO-S-001', DATE '2026-10-03', 'gb.cashier.1', 'CUS-01', 'GB-SKU-SALT-1KG', 1, 'cash'),
        ('GB-DEMO-S-002', DATE '2026-10-04', 'gb.cashier.2', 'CUS-02', 'GB-SKU-RICE-IG-1KG', 1, 'upi'),
        ('GB-DEMO-S-002', DATE '2026-10-04', 'gb.cashier.2', 'CUS-02', 'GB-SKU-ATTA-1KG', 1, 'upi'),
        ('GB-DEMO-S-003', DATE '2026-10-04', 'gb.cashier.senior', NULL, 'GB-SKU-RICE-IG-5KG', 1, 'card'),
        ('GB-DEMO-S-003', DATE '2026-10-04', 'gb.cashier.senior', NULL, 'GB-SKU-SUN-1L', 1, 'card'),
        ('GB-DEMO-S-004', DATE '2026-10-05', 'gb.cashier.1', 'CUS-03', 'GB-SKU-MATTA-1KG', 1, 'cash'),
        ('GB-DEMO-S-004', DATE '2026-10-05', 'gb.cashier.1', 'CUS-03', 'GB-SKU-SUGAR-1KG', 1, 'cash'),
        ('GB-DEMO-S-005', DATE '2026-10-05', 'gb.cashier.2', 'CUS-04', 'GB-SKU-MILK-1L', 1, 'upi'),
        ('GB-DEMO-S-005', DATE '2026-10-05', 'gb.cashier.2', 'CUS-04', 'GB-SKU-CURD-500', 1, 'upi'),
        ('GB-DEMO-S-006', DATE '2026-10-06', 'gb.cashier.senior', 'CUS-05', 'GB-SKU-TEA-250', 1, 'cash'),
        ('GB-DEMO-S-006', DATE '2026-10-06', 'gb.cashier.senior', 'CUS-05', 'GB-SKU-COFFEE-200', 1, 'cash'),
        ('GB-DEMO-S-007', DATE '2026-10-06', 'gb.cashier.1', NULL, 'GB-SKU-CHIPS-52', 1, 'card'),
        ('GB-DEMO-S-007', DATE '2026-10-06', 'gb.cashier.1', NULL, 'GB-SKU-BISCUIT-250', 1, 'card'),
        ('GB-DEMO-S-008', DATE '2026-10-07', 'gb.cashier.2', 'CUS-06', 'GB-SKU-SOAP-1', 1, 'cash'),
        ('GB-DEMO-S-008', DATE '2026-10-07', 'gb.cashier.2', 'CUS-06', 'GB-SKU-TOOTH-150', 1, 'cash'),
        ('GB-DEMO-S-009', DATE '2026-10-07', 'gb.sales', 'CUS-07', 'GB-SKU-WASH-1KG', 1, 'upi'),
        ('GB-DEMO-S-009', DATE '2026-10-07', 'gb.sales', 'CUS-07', 'GB-SKU-DISH-500', 1, 'upi'),
        ('GB-DEMO-S-010', DATE '2026-10-08', 'gb.cashier.1', NULL, 'GB-SKU-OLIVE-500', 2, 'cash'),
        ('GB-DEMO-S-010', DATE '2026-10-08', 'gb.cashier.1', NULL, 'GB-SKU-JAGGERY-500', 1, 'cash'),
        ('GB-DEMO-S-011', DATE '2026-10-08', 'gb.cashier.senior', 'CUS-08', 'GB-SKU-SHOE-BK-9', 1, 'card'),
        ('GB-DEMO-S-011', DATE '2026-10-08', 'gb.cashier.senior', 'CUS-08', 'GB-SKU-SHOE-TN-8', 1, 'card'),
        ('GB-DEMO-S-012', DATE '2026-10-09', 'gb.cashier.2', 'CUS-09', 'GB-SKU-SHOE-BK-9', 1, 'cash'),
        ('GB-DEMO-S-012', DATE '2026-10-09', 'gb.cashier.2', 'CUS-09', 'GB-SKU-WATER-1L', 1, 'cash'),
        ('GB-DEMO-S-013', DATE '2026-10-09', 'gb.cashier.1', NULL, 'GB-SKU-SHOE-BK-9', 1, 'upi'),
        ('GB-DEMO-S-013', DATE '2026-10-09', 'gb.cashier.1', NULL, 'GB-SKU-BREAD-400', 1, 'upi'),
        ('GB-DEMO-S-014', DATE '2026-10-10', 'gb.sales', 'CUS-10', 'GB-SKU-OLIVE-500', 2, 'cash'),
        ('GB-DEMO-S-014', DATE '2026-10-10', 'gb.sales', 'CUS-10', 'GB-SKU-HONEY-250', 1, 'cash'),
        ('GB-DEMO-S-015', DATE '2026-10-10', 'gb.cashier.2', 'CUS-11', 'GB-SKU-WASH-2KG', 2, 'card'),
        ('GB-DEMO-S-015', DATE '2026-10-10', 'gb.cashier.2', 'CUS-11', 'GB-SKU-WASH-1KG', 1, 'card'),
        ('GB-DEMO-S-016', DATE '2026-10-11', 'gb.cashier.senior', NULL, 'GB-SKU-WASH-2KG', 2, 'cash'),
        ('GB-DEMO-S-016', DATE '2026-10-11', 'gb.cashier.senior', NULL, 'GB-SKU-FLOOR-1L', 1, 'cash'),
        ('GB-DEMO-S-017', DATE '2026-10-12', 'gb.cashier.1', 'CUS-12', 'GB-SKU-WASH-2KG', 2, 'upi'),
        ('GB-DEMO-S-017', DATE '2026-10-12', 'gb.cashier.1', 'CUS-12', 'GB-SKU-SHAMPOO-180', 1, 'upi'),
        ('GB-DEMO-S-018', DATE '2026-10-13', 'gb.cashier.2', 'CUS-13', 'GB-SKU-WASH-2KG', 2, 'cash'),
        ('GB-DEMO-S-018', DATE '2026-10-13', 'gb.cashier.2', 'CUS-13', 'GB-SKU-SALT-1KG', 2, 'cash'),
        ('GB-DEMO-S-019', DATE '2026-10-14', 'gb.cashier.senior', 'CUS-14', 'GB-SKU-SUGAR-1KG', 1, 'card'),
        ('GB-DEMO-S-019', DATE '2026-10-14', 'gb.cashier.senior', 'CUS-14', 'GB-SKU-RICE-IG-1KG', 1, 'card'),
        ('GB-DEMO-S-020', DATE '2026-10-15', 'gb.cashier.1', 'CUS-15', 'GB-SKU-MILK-500', 1, 'cash'),
        ('GB-DEMO-S-020', DATE '2026-10-15', 'gb.cashier.1', 'CUS-15', 'GB-SKU-BUTTER-100', 1, 'cash'),
        ('GB-DEMO-S-021', DATE '2026-10-16', 'gb.sales', NULL, 'GB-SKU-NOODLE-70', 1, 'upi'),
        ('GB-DEMO-S-021', DATE '2026-10-16', 'gb.sales', NULL, 'GB-SKU-PASTA-500', 1, 'upi'),
        ('GB-DEMO-S-022', DATE '2026-10-17', 'gb.cashier.2', 'CUS-01', 'GB-SKU-GHEE-200', 1, 'cash'),
        ('GB-DEMO-S-022', DATE '2026-10-17', 'gb.cashier.2', 'CUS-01', 'GB-SKU-BANANA-200', 1, 'cash');

    IF (SELECT COUNT(DISTINCT invoice) FROM gb_sale) < 20 THEN
        RAISE EXCEPTION 'Fewer than 20 demo sales were staged.';
    END IF;

    INSERT INTO sales (
        fk_store, fk_customer, fk_cashsession, invoicenumber, saledate,
        subtotal, taxamount, discountamount, totalamount, paidamount, changegiven,
        status, notes, createdby
    )
    SELECT
        v_store,
        cu.id_customer,
        v_session,
        s.invoice,
        s.sold_on::timestamp + TIME '11:00',
        SUM(ROUND(s.qty * (gp.sellingprice + sel.add_price), 2)),
        SUM(ROUND(s.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2)),
        0,
        SUM(ROUND(s.qty * (gp.sellingprice + sel.add_price), 2))
            + SUM(ROUND(s.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2)),
        SUM(ROUND(s.qty * (gp.sellingprice + sel.add_price), 2))
            + SUM(ROUND(s.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2)),
        0,
        'completed',
        'GB-DEMO|sale ' || s.invoice,
        usr.id_user
    FROM gb_sale s
    JOIN gb_sell sel ON sel.item_key = s.item_key
    JOIN gb_product gp ON gp.code = sel.product_code
    JOIN taxes t ON t.taxcode = gp.taxcode AND t.cancelled = FALSE
    JOIN users usr ON usr.username = s.cashier AND usr.cancelled = FALSE
    LEFT JOIN customers cu ON cu.customercode = s.customer_code AND cu.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM sales x WHERE x.fk_store = v_store AND x.invoicenumber = s.invoice
    )
    GROUP BY cu.id_customer, s.invoice, s.sold_on, usr.id_user;

    INSERT INTO sale_details (
        fk_sale, fk_store, fk_product, fk_productvariant,
        quantity, unitprice, taxamount, discountamount, linetotal
    )
    SELECT
        sa.id_sale,
        v_store,
        sel.id_product,
        sel.id_variant,
        s.qty,
        gp.sellingprice + sel.add_price,
        ROUND(s.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2),
        0,
        ROUND(s.qty * (gp.sellingprice + sel.add_price), 2)
            + ROUND(s.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2)
    FROM gb_sale s
    JOIN sales sa ON sa.fk_store = v_store AND sa.invoicenumber = s.invoice
    JOIN gb_sell sel ON sel.item_key = s.item_key
    JOIN gb_product gp ON gp.code = sel.product_code
    JOIN taxes t ON t.taxcode = gp.taxcode AND t.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM sale_details d
        WHERE d.fk_sale = sa.id_sale
          AND d.fk_product = sel.id_product
          AND d.fk_productvariant IS NOT DISTINCT FROM sel.id_variant
    );

    INSERT INTO payments (
        fk_store, fk_sale, fk_cashsession, paymentmethod, amount, referenceno, paidat, createdby
    )
    SELECT
        v_store, sa.id_sale, v_session, s.pay_method, sa.totalamount,
        CASE WHEN s.pay_method = 'cash' THEN NULL ELSE 'GB-DEMO-PAY-' || sa.invoicenumber END,
        sa.saledate, sa.createdby
    FROM sales sa
    JOIN (
        SELECT invoice, MIN(pay_method) AS pay_method
        FROM gb_sale
        GROUP BY invoice
    ) s ON s.invoice = sa.invoicenumber
    WHERE sa.fk_store = v_store
      AND sa.notes LIKE 'GB-DEMO|sale %'
      AND NOT EXISTS (SELECT 1 FROM payments p WHERE p.fk_sale = sa.id_sale);

    UPDATE cash_sessions c
    SET expectedcash = 2000 + COALESCE((
            SELECT SUM(p.amount)
            FROM payments p
            WHERE p.fk_cashsession = c.id_cashsession
              AND p.paymentmethod = 'cash'
              AND p.cancelled = FALSE
        ), 0),
        closingcash = 2000 + COALESCE((
            SELECT SUM(p.amount)
            FROM payments p
            WHERE p.fk_cashsession = c.id_cashsession
              AND p.paymentmethod = 'cash'
              AND p.cancelled = FALSE
        ), 0),
        variance = 0
    WHERE c.id_cashsession = v_session;

    INSERT INTO stock_movements (
        fk_store, fk_product, fk_productvariant, movementtype, quantity,
        referencetype, referenceid, notes, createdby, createdat
    )
    SELECT
        v_store, d.fk_product, d.fk_productvariant, 'sale', d.quantity,
        'sale', sa.id_sale, 'GB-DEMO|sale ' || sa.invoicenumber, sa.createdby, sa.saledate
    FROM sales sa
    JOIN sale_details d ON d.fk_sale = sa.id_sale
    WHERE sa.fk_store = v_store
      AND sa.notes LIKE 'GB-DEMO|sale %'
      AND NOT EXISTS (
          SELECT 1 FROM stock_movements m
          WHERE m.movementtype = 'sale' AND m.referencetype = 'sale' AND m.referenceid = sa.id_sale
            AND m.fk_product = d.fk_product
            AND m.fk_productvariant IS NOT DISTINCT FROM d.fk_productvariant
      );

    CREATE TEMP TABLE gb_return (
        return_no TEXT NOT NULL,
        invoice TEXT NOT NULL,
        item_key TEXT NOT NULL,
        qty NUMERIC(14,3) NOT NULL,
        reason TEXT NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO gb_return (return_no, invoice, item_key, qty, reason) VALUES
        ('GB-DEMO-R-001', 'GB-DEMO-S-001', 'GB-SKU-RICE-IG-1KG', 1, 'Demo return: customer changed their mind.'),
        ('GB-DEMO-R-002', 'GB-DEMO-S-005', 'GB-SKU-CURD-500', 1, 'Demo return: pack damaged.'),
        ('GB-DEMO-R-003', 'GB-DEMO-S-007', 'GB-SKU-CHIPS-52', 1, 'Demo return: wrong item taken home.');

    INSERT INTO sales_returns (
        fk_store, fk_sale, returnnumber, returndate, totalamount, reason, status, createdby
    )
    SELECT
        v_store,
        sa.id_sale,
        r.return_no,
        sa.saledate + INTERVAL '1 day',
        ROUND(SUM(r.qty * d.unitprice), 2),
        MIN(r.reason),
        'completed',
        (SELECT id_user FROM users WHERE username = 'gb.cashier.senior' AND cancelled = FALSE)
    FROM gb_return r
    JOIN sales sa ON sa.fk_store = v_store AND sa.invoicenumber = r.invoice
    JOIN gb_sell sel ON sel.item_key = r.item_key
    JOIN sale_details d ON d.fk_sale = sa.id_sale
        AND d.fk_product = sel.id_product
        AND d.fk_productvariant IS NOT DISTINCT FROM sel.id_variant
    WHERE r.qty <= d.quantity
      AND NOT EXISTS (
          SELECT 1 FROM sales_returns x
          WHERE x.fk_store = v_store AND x.returnnumber = r.return_no
      )
    GROUP BY sa.id_sale, r.return_no, sa.saledate;

    IF (SELECT COUNT(*) FROM sales_returns WHERE fk_store = v_store AND returnnumber LIKE 'GB-DEMO-R-%') < 3 THEN
        RAISE EXCEPTION 'Demo returns were not created. A return may exceed the original line.';
    END IF;

    INSERT INTO sales_return_details (
        fk_salesreturn, fk_store, fk_product, fk_productvariant, quantity, unitprice, linetotal
    )
    SELECT
        sr.id_salesreturn, v_store, sel.id_product, sel.id_variant, r.qty, d.unitprice,
        ROUND(r.qty * d.unitprice, 2)
    FROM gb_return r
    JOIN sales sa ON sa.fk_store = v_store AND sa.invoicenumber = r.invoice
    JOIN sales_returns sr ON sr.fk_sale = sa.id_sale AND sr.returnnumber = r.return_no
    JOIN gb_sell sel ON sel.item_key = r.item_key
    JOIN sale_details d ON d.fk_sale = sa.id_sale
        AND d.fk_product = sel.id_product
        AND d.fk_productvariant IS NOT DISTINCT FROM sel.id_variant
    WHERE NOT EXISTS (
        SELECT 1 FROM sales_return_details x
        WHERE x.fk_salesreturn = sr.id_salesreturn
          AND x.fk_product = sel.id_product
          AND x.fk_productvariant IS NOT DISTINCT FROM sel.id_variant
    );

    INSERT INTO stock_movements (
        fk_store, fk_product, fk_productvariant, movementtype, quantity,
        referencetype, referenceid, notes, createdby, createdat
    )
    SELECT
        v_store, d.fk_product, d.fk_productvariant, 'return_in', d.quantity,
        'sales_return', sr.id_salesreturn,
        'GB-DEMO|return ' || sr.returnnumber,
        sr.createdby, sr.returndate
    FROM sales_returns sr
    JOIN sales_return_details d ON d.fk_salesreturn = sr.id_salesreturn
    WHERE sr.fk_store = v_store
      AND sr.returnnumber LIKE 'GB-DEMO-R-%'
      AND NOT EXISTS (
          SELECT 1 FROM stock_movements m
          WHERE m.movementtype = 'return_in'
            AND m.referencetype = 'sales_return'
            AND m.referenceid = sr.id_salesreturn
            AND m.fk_product = d.fk_product
            AND m.fk_productvariant IS NOT DISTINCT FROM d.fk_productvariant
      );

    CREATE TEMP TABLE gb_order (
        order_no TEXT PRIMARY KEY,
        customer_code TEXT NOT NULL,
        status TEXT NOT NULL,
        was_confirmed BOOLEAN NOT NULL,
        ordered_on TIMESTAMP NOT NULL,
        address TEXT NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO gb_order (order_no, customer_code, status, was_confirmed, ordered_on, address) VALUES
        ('GB-DEMO-O-001', 'CUS-01', 'pending', FALSE, TIMESTAMP '2026-10-18 10:00', 'Panampilly Nagar, Kochi'),
        ('GB-DEMO-O-002', 'CUS-02', 'pending', FALSE, TIMESTAMP '2026-10-18 11:00', 'Kadavanthra, Kochi'),
        ('GB-DEMO-O-003', 'CUS-03', 'confirmed', TRUE, TIMESTAMP '2026-10-16 12:00', 'Fort Kochi'),
        ('GB-DEMO-O-004', 'CUS-04', 'packed', TRUE, TIMESTAMP '2026-10-16 13:00', 'Kacheripady, Ernakulam'),
        ('GB-DEMO-O-005', 'CUS-05', 'shipped', TRUE, TIMESTAMP '2026-10-15 14:00', 'Mattancherry, Kochi'),
        ('GB-DEMO-O-006', 'CUS-06', 'delivered', TRUE, TIMESTAMP '2026-10-14 15:00', 'Vyttila, Kochi'),
        ('GB-DEMO-O-007', 'CUS-07', 'delivered', TRUE, TIMESTAMP '2026-10-13 16:00', 'Edappally, Kochi'),
        ('GB-DEMO-O-008', 'CUS-08', 'cancelled', FALSE, TIMESTAMP '2026-10-17 17:00', 'Kaloor, Kochi'),
        ('GB-DEMO-O-009', 'CUS-09', 'cancelled', TRUE, TIMESTAMP '2026-10-12 18:00', 'Tripunithura, Kochi'),
        ('GB-DEMO-O-010', 'CUS-10', 'packed', TRUE, TIMESTAMP '2026-10-15 19:00', 'Aluva, Kochi');

    CREATE TEMP TABLE gb_order_line (
        order_no TEXT NOT NULL,
        item_key TEXT NOT NULL,
        qty NUMERIC(14,3) NOT NULL,
        PRIMARY KEY (order_no, item_key)
    ) ON COMMIT DROP;

    INSERT INTO gb_order_line (order_no, item_key, qty) VALUES
        ('GB-DEMO-O-001', 'GB-SKU-RICE-IG-5KG', 1),
        ('GB-DEMO-O-001', 'GB-SKU-SALT-1KG', 1),
        ('GB-DEMO-O-002', 'GB-SKU-MILK-500', 1),
        ('GB-DEMO-O-003', 'GB-SKU-RICE-IG-1KG', 1),
        ('GB-DEMO-O-004', 'GB-SKU-ATTA-1KG', 1),
        ('GB-DEMO-O-005', 'GB-SKU-WASH-2KG', 2),
        ('GB-DEMO-O-006', 'GB-SKU-TEA-250', 1),
        ('GB-DEMO-O-006', 'GB-SKU-WATER-1L', 1),
        ('GB-DEMO-O-007', 'GB-SKU-BISCUIT-250', 1),
        ('GB-DEMO-O-008', 'GB-SKU-CHIPS-52', 1),
        ('GB-DEMO-O-009', 'GB-SKU-RICE-IG-1KG', 1),
        ('GB-DEMO-O-010', 'GB-SKU-WASH-2KG', 2);

    INSERT INTO online_orders (
        fk_store, fk_customer, ordernumber, orderdate, status,
        subtotal, taxamount, shippingamount, discountamount, totalamount,
        shippingaddress, notes, cancelled, cancelledon, cancelledreason
    )
    SELECT
        v_store,
        cu.id_customer,
        o.order_no,
        o.ordered_on,
        o.status,
        SUM(ROUND(l.qty * (gp.sellingprice + sel.add_price), 2)),
        SUM(ROUND(l.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2)),
        0, 0,
        SUM(ROUND(l.qty * (gp.sellingprice + sel.add_price), 2))
            + SUM(ROUND(l.qty * (gp.sellingprice + sel.add_price) * t.taxrate / 100, 2)),
        o.address,
        'GB-DEMO|online ' || o.order_no || CASE
            WHEN o.status = 'pending' THEN '. No stock movement.'
            WHEN o.status = 'cancelled' AND o.was_confirmed = FALSE THEN '. Cancelled before confirm. No stock movement.'
            WHEN o.status = 'cancelled' AND o.was_confirmed = TRUE THEN '. Cancelled after confirm. Sale and return_in net to zero.'
            ELSE '. Stock deducted once at confirm.'
        END,
        o.status = 'cancelled',
        CASE WHEN o.status = 'cancelled' THEN o.ordered_on + INTERVAL '2 hours' ELSE NULL END,
        CASE
            WHEN o.status = 'cancelled' AND o.was_confirmed = FALSE THEN 'Demo cancel before confirm.'
            WHEN o.status = 'cancelled' AND o.was_confirmed = TRUE THEN 'Demo cancel after confirm.'
            ELSE NULL
        END
    FROM gb_order o
    JOIN customers cu ON cu.customercode = o.customer_code AND cu.cancelled = FALSE
    JOIN gb_order_line l ON l.order_no = o.order_no
    JOIN gb_sell sel ON sel.item_key = l.item_key
    JOIN gb_product gp ON gp.code = sel.product_code
    JOIN taxes t ON t.taxcode = gp.taxcode AND t.cancelled = FALSE
    WHERE NOT EXISTS (
        SELECT 1 FROM online_orders x
        WHERE x.fk_store = v_store AND x.ordernumber = o.order_no
    )
    GROUP BY cu.id_customer, o.order_no, o.ordered_on, o.status, o.was_confirmed, o.address;

    INSERT INTO online_order_items (
        fk_onlineorder, fk_store, fk_product, fk_productvariant, quantity, unitprice, linetotal
    )
    SELECT
        oo.id_onlineorder, v_store, sel.id_product, sel.id_variant, l.qty,
        gp.sellingprice + sel.add_price,
        ROUND(l.qty * (gp.sellingprice + sel.add_price), 2)
    FROM gb_order_line l
    JOIN online_orders oo ON oo.fk_store = v_store AND oo.ordernumber = l.order_no
    JOIN gb_sell sel ON sel.item_key = l.item_key
    JOIN gb_product gp ON gp.code = sel.product_code
    WHERE NOT EXISTS (
        SELECT 1 FROM online_order_items i
        WHERE i.fk_onlineorder = oo.id_onlineorder
          AND i.fk_product = sel.id_product
          AND i.fk_productvariant IS NOT DISTINCT FROM sel.id_variant
    );

    INSERT INTO stock_movements (
        fk_store, fk_product, fk_productvariant, movementtype, quantity,
        referencetype, referenceid, notes, createdby, createdat
    )
    SELECT
        v_store, i.fk_product, i.fk_productvariant, 'sale', i.quantity,
        'online_order', oo.id_onlineorder,
        'GB-DEMO|online confirm ' || oo.ordernumber,
        v_admin, oo.orderdate
    FROM online_orders oo
    JOIN gb_order g ON g.order_no = oo.ordernumber AND g.was_confirmed = TRUE
    JOIN online_order_items i ON i.fk_onlineorder = oo.id_onlineorder
    WHERE oo.fk_store = v_store
      AND NOT EXISTS (
          SELECT 1 FROM stock_movements m
          WHERE m.movementtype = 'sale' AND m.referencetype = 'online_order' AND m.referenceid = oo.id_onlineorder
            AND m.fk_product = i.fk_product
            AND m.fk_productvariant IS NOT DISTINCT FROM i.fk_productvariant
      );

    INSERT INTO stock_movements (
        fk_store, fk_product, fk_productvariant, movementtype, quantity,
        referencetype, referenceid, notes, createdby, createdat
    )
    SELECT
        v_store, i.fk_product, i.fk_productvariant, 'return_in', i.quantity,
        'online_order', oo.id_onlineorder,
        'GB-DEMO|online cancel ' || oo.ordernumber,
        v_admin, oo.cancelledon
    FROM online_orders oo
    JOIN gb_order g ON g.order_no = oo.ordernumber
        AND g.was_confirmed = TRUE AND g.status = 'cancelled'
    JOIN online_order_items i ON i.fk_onlineorder = oo.id_onlineorder
    WHERE oo.fk_store = v_store
      AND NOT EXISTS (
          SELECT 1 FROM stock_movements m
          WHERE m.movementtype = 'return_in' AND m.referencetype = 'online_order' AND m.referenceid = oo.id_onlineorder
            AND m.fk_product = i.fk_product
            AND m.fk_productvariant IS NOT DISTINCT FROM i.fk_productvariant
      );

    INSERT INTO inventory (
        fk_store, fk_product, fk_productvariant, quantityonhand, quantityreserved, reorderlevel
    )
    SELECT
        v_store,
        m.fk_product,
        m.fk_productvariant,
        SUM(CASE
            WHEN m.movementtype IN ('purchase', 'return_in', 'opening', 'transfer_in') THEN m.quantity
            WHEN m.movementtype IN ('sale', 'return_out', 'transfer_out') THEN -m.quantity
            ELSE 0
        END),
        0,
        COALESCE(MAX(s.reorder_level), 0)
    FROM stock_movements m
    LEFT JOIN gb_sell s ON s.id_product = m.fk_product
        AND s.id_variant IS NOT DISTINCT FROM m.fk_productvariant
    WHERE m.fk_store = v_store
      AND m.notes LIKE 'GB-DEMO|%'
    GROUP BY m.fk_product, m.fk_productvariant
    HAVING NOT EXISTS (
        SELECT 1 FROM inventory i
        WHERE i.fk_store = v_store
          AND i.fk_product = m.fk_product
          AND i.fk_productvariant IS NOT DISTINCT FROM m.fk_productvariant
    );

    SELECT COUNT(*) INTO v_neg
    FROM (
        SELECT SUM(CASE
            WHEN movementtype IN ('purchase', 'return_in', 'opening', 'transfer_in') THEN quantity
            WHEN movementtype IN ('sale', 'return_out', 'transfer_out') THEN -quantity
            ELSE 0
        END) AS net
        FROM stock_movements
        WHERE fk_store = v_store AND notes LIKE 'GB-DEMO|%'
        GROUP BY fk_product, fk_productvariant
    ) n
    WHERE n.net < 0;

    IF v_neg > 0 THEN
        RAISE EXCEPTION 'Demo stock would go negative. Patch rolled back.';
    END IF;

    SELECT COUNT(*) INTO v_parent
    FROM inventory i
    JOIN products p ON p.id_product = i.fk_product
    WHERE i.fk_store = v_store
      AND p.productcode LIKE 'GB-%'
      AND i.fk_productvariant IS NULL
      AND EXISTS (
          SELECT 1 FROM product_variants v
          WHERE v.fk_product = p.id_product AND v.cancelled = FALSE
      );

    IF v_parent > 0 THEN
        RAISE EXCEPTION 'A variant parent has its own stock row. Patch rolled back.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM inventory i
        JOIN (
            SELECT fk_product, fk_productvariant,
                SUM(CASE
                    WHEN movementtype IN ('purchase', 'return_in', 'opening', 'transfer_in') THEN quantity
                    WHEN movementtype IN ('sale', 'return_out', 'transfer_out') THEN -quantity
                    ELSE 0
                END) AS net
            FROM stock_movements
            WHERE fk_store = v_store AND notes LIKE 'GB-DEMO|%'
            GROUP BY fk_product, fk_productvariant
        ) m ON m.fk_product = i.fk_product
           AND m.fk_productvariant IS NOT DISTINCT FROM i.fk_productvariant
        WHERE i.fk_store = v_store
          AND i.quantityonhand <> m.net
    ) THEN
        RAISE EXCEPTION 'Inventory does not match demo movements. Patch rolled back.';
    END IF;

    INSERT INTO carts (fk_store, fk_customer, sessionkey)
    SELECT v_store, cu.id_customer, NULL
    FROM customers cu
    WHERE cu.customercode = 'CUS-11' AND cu.cancelled = FALSE
      AND NOT EXISTS (
          SELECT 1 FROM carts c WHERE c.fk_store = v_store AND c.fk_customer = cu.id_customer
      );

    INSERT INTO cart_items (fk_cart, fk_store, fk_product, fk_productvariant, quantity)
    SELECT c.id_cart, v_store, s.id_product, s.id_variant, v.qty
    FROM (VALUES
        ('GB-SKU-RICE-IG-1KG', 2),
        ('GB-SKU-MILK-1L', 1)
    ) AS v(item_key, qty)
    JOIN gb_sell s ON s.item_key = v.item_key
    JOIN customers cu ON cu.customercode = 'CUS-11' AND cu.cancelled = FALSE
    JOIN carts c ON c.fk_store = v_store AND c.fk_customer = cu.id_customer
    WHERE NOT EXISTS (
        SELECT 1 FROM cart_items i
        WHERE i.fk_cart = c.id_cart
          AND i.fk_product = s.id_product
          AND i.fk_productvariant IS NOT DISTINCT FROM s.id_variant
    );

    INSERT INTO wishlists (fk_store, fk_customer)
    SELECT v_store, cu.id_customer
    FROM customers cu
    WHERE cu.customercode = 'CUS-12' AND cu.cancelled = FALSE
      AND NOT EXISTS (
          SELECT 1 FROM wishlists w WHERE w.fk_store = v_store AND w.fk_customer = cu.id_customer
      );

    INSERT INTO wishlist_items (fk_wishlist, fk_store, fk_product)
    SELECT w.id_wishlist, v_store, p.id_product
    FROM gb_product p
    JOIN customers cu ON cu.customercode = 'CUS-12' AND cu.cancelled = FALSE
    JOIN wishlists w ON w.fk_store = v_store AND w.fk_customer = cu.id_customer
    WHERE p.code IN ('GB-CHIPS', 'GB-SOAP')
      AND NOT EXISTS (
          SELECT 1 FROM wishlist_items i
          WHERE i.fk_wishlist = w.id_wishlist AND i.fk_product = p.id_product
      );

    INSERT INTO product_reviews (
        fk_product, fk_customer, fk_store, fk_onlineorder, fk_onlineorderitem,
        rating, title, reviewtext, isapproved
    )
    SELECT
        i.fk_product, oo.fk_customer, v_store, oo.id_onlineorder, i.id_onlineorderitem,
        v.rating, v.title, v.reviewtext, v.isapproved
    FROM (VALUES
        ('GB-DEMO-O-006', 'GB-SKU-TEA-250', 5, 'Good tea', 'Demo review for a delivered order.', TRUE),
        ('GB-DEMO-O-007', 'GB-SKU-BISCUIT-250', 4, 'Crisp biscuits', 'Demo review waiting for approval.', FALSE)
    ) AS v(order_no, item_key, rating, title, reviewtext, isapproved)
    JOIN online_orders oo ON oo.fk_store = v_store AND oo.ordernumber = v.order_no AND oo.status = 'delivered'
    JOIN gb_sell s ON s.item_key = v.item_key
    JOIN online_order_items i ON i.fk_onlineorder = oo.id_onlineorder
        AND i.fk_product = s.id_product
        AND i.fk_productvariant IS NOT DISTINCT FROM s.id_variant
    WHERE NOT EXISTS (
        SELECT 1 FROM product_reviews r
        WHERE r.fk_customer = oo.fk_customer AND r.fk_onlineorderitem = i.id_onlineorderitem
    );

    INSERT INTO expenses (fk_store, expensedate, category, description, amount, paymentmethod, createdby)
    SELECT v_store, v.expensedate, v.category, v.description, v.amount, 'cash', v_admin
    FROM (VALUES
        (DATE '2026-10-08', 'Utilities', 'GB-DEMO|shop electricity sample', 2450.00),
        (DATE '2026-10-12', 'Housekeeping', 'GB-DEMO|cleaning supplies sample', 680.00),
        (DATE '2026-10-15', 'Transport', 'GB-DEMO|local delivery sample', 900.00)
    ) AS v(expensedate, category, description, amount)
    WHERE NOT EXISTS (
        SELECT 1 FROM expenses e WHERE e.fk_store = v_store AND e.description = v.description
    );

    INSERT INTO audit_logs (fk_store, fk_user, entityname, entityid, action, details)
    SELECT v_store, v_admin, 'DemoSeed', v_store, 'Seed',
           'GB-DEMO|Green Basket supermarket sample data loaded for manual testing.'
    WHERE NOT EXISTS (
        SELECT 1 FROM audit_logs a
        WHERE a.entityname = 'DemoSeed' AND a.details LIKE 'GB-DEMO|Green Basket supermarket%'
    );

    INSERT INTO user_invitations (
        fk_user, invited_email, invited_fullname, invitation_token,
        fk_role, fk_store, status, expires_at, accepted_at, delivery_status
    )
    SELECT
        u.id_user, u.email, u.fullname, 'gb-demo-accepted-not-a-login-token',
        r.id_role, v_store, 'accepted', TIMESTAMP '2026-10-01 09:00:00',
        TIMESTAMP '2026-10-01 09:05:00', 'sent'
    FROM users u
    JOIN roles r ON r.rolecode = 'ADMIN' AND r.cancelled = FALSE
    WHERE u.username = 'gb.admin' AND u.cancelled = FALSE
      AND NOT EXISTS (
          SELECT 1 FROM user_invitations i
          WHERE i.invitation_token = 'gb-demo-accepted-not-a-login-token'
      );

    INSERT INTO invoice_sequences (fk_store, sequencetype, prefix, nextnumber, padding)
    SELECT v_store, v.sequencetype, '', 1, 6
    FROM (VALUES ('SALE'), ('RETURN'), ('GRN')) AS v(sequencetype)
    WHERE NOT EXISTS (
        SELECT 1 FROM invoice_sequences q
        WHERE q.fk_store = v_store AND q.sequencetype = v.sequencetype
    );

    INSERT INTO settings (setting_scope, fk_store, settingkey, settingvalue, valuetype, description)
    SELECT 'store', v_store, 'pos.allow_negative_stock', 'false', 'bool',
           'GB demo store does not allow negative stock.'
    WHERE NOT EXISTS (
        SELECT 1 FROM settings s
        WHERE s.setting_scope = 'store' AND s.fk_store = v_store AND s.settingkey = 'pos.allow_negative_stock'
    );

    INSERT INTO module_presets (presetcode, presetname, description, displayorder)
    SELECT v.presetcode, v.presetname, v.description, v.displayorder
    FROM (VALUES
        ('BASIC', 'Basic', 'Core modules: Dashboard, POS, Products, Inventory', 10),
        ('PREMIUM', 'Premium', 'All supported modules including Reports and User Management', 20)
    ) AS v(presetcode, presetname, description, displayorder)
    WHERE NOT EXISTS (
        SELECT 1 FROM module_presets p WHERE p.presetcode = v.presetcode
    );

    INSERT INTO module_preset_items (fk_preset, fk_module)
    SELECT p.id_preset, m.id_module
    FROM module_presets p
    JOIN modules m ON m.cancelled = FALSE
    WHERE (
            (p.presetcode = 'BASIC' AND m.modulecode IN ('DASHBOARD','PRODUCTS','POS','INVENTORY'))
         OR (p.presetcode = 'PREMIUM' AND m.modulecode IN (
                'DASHBOARD','PRODUCTS','INVENTORY','PURCHASES','POS',
                'CUSTOMERS','REPORTS','SETTINGS','USERS'))
          )
      AND NOT EXISTS (
          SELECT 1 FROM module_preset_items i
          WHERE i.fk_preset = p.id_preset AND i.fk_module = m.id_module
      );

    INSERT INTO settings (setting_scope, fk_store, settingkey, settingvalue, valuetype, description)
    SELECT 'company', NULL, v.settingkey, v.settingvalue, v.valuetype, v.description
    FROM (VALUES
        ('pos.invoice_prefix', 'INV', 'string', 'Default POS invoice prefix'),
        ('pos.allow_negative_stock', 'false', 'bool', 'Allow selling below zero stock'),
        ('inventory.track_variants', 'true', 'bool', 'Track stock per product variant'),
        ('locale.currency', 'INR', 'string', 'Default currency code'),
        ('locale.timezone', 'Asia/Kolkata', 'string', 'Default timezone')
    ) AS v(settingkey, settingvalue, valuetype, description)
    WHERE NOT EXISTS (
        SELECT 1 FROM settings s
        WHERE s.setting_scope = 'company'
          AND s.fk_store IS NULL
          AND s.settingkey = v.settingkey
    );

    INSERT INTO settings (setting_scope, fk_store, settingkey, settingvalue, valuetype, description)
    SELECT 'company', NULL, 'gb.demo.dataset', 'v1', 'string',
           'Marker that V010 Green Basket demo data is loaded. Do not delete unless you intend to load it again.'
    WHERE NOT EXISTS (
        SELECT 1 FROM settings s
        WHERE s.setting_scope = 'company' AND s.fk_store IS NULL AND s.settingkey = 'gb.demo.dataset'
    );

    RAISE NOTICE 'Green Basket demo data inserted. Products %, variants %, store %.',
        (SELECT COUNT(*) FROM gb_product),
        (SELECT COUNT(*) FROM gb_sell WHERE variant_name IS NOT NULL),
        v_store;
END
$gb$;

COMMIT;

-- Verification queries are comments so this file does not print result sets
-- when you apply it. Copy them after the load.
--
-- SELECT COUNT(*) FROM products WHERE productcode LIKE 'GB-%' AND cancelled = FALSE;
-- SELECT COUNT(*) FROM product_variants v JOIN products p ON p.id_product = v.fk_product WHERE p.productcode LIKE 'GB-%';
-- SELECT sku, COUNT(*) FROM (
--   SELECT sku AS sku FROM products WHERE sku IS NOT NULL
--   UNION ALL
--   SELECT skucode FROM product_variants WHERE skucode IS NOT NULL
-- ) s GROUP BY sku HAVING COUNT(*) > 1;
-- SELECT barcode, COUNT(*) FROM (
--   SELECT barcode FROM products WHERE barcode IS NOT NULL
--   UNION ALL
--   SELECT barcode FROM product_variants WHERE barcode IS NOT NULL
-- ) s GROUP BY barcode HAVING COUNT(*) > 1;
-- SELECT p.productcode, v.variantname, i.quantityonhand
-- FROM inventory i
-- JOIN products p ON p.id_product = i.fk_product
-- LEFT JOIN product_variants v ON v.id_productvariant = i.fk_productvariant
-- WHERE p.productcode LIKE 'GB-%'
-- ORDER BY p.productcode, v.variantname;
-- SELECT i.fk_product, i.fk_productvariant, i.quantityonhand, m.net
-- FROM inventory i
-- JOIN (
--   SELECT fk_product, fk_productvariant,
--     SUM(CASE WHEN movementtype IN ('purchase','return_in','opening','transfer_in') THEN quantity
--              WHEN movementtype IN ('sale','return_out','transfer_out') THEN -quantity ELSE 0 END) AS net
--   FROM stock_movements WHERE notes LIKE 'GB-DEMO|%'
--   GROUP BY fk_product, fk_productvariant
-- ) m ON m.fk_product = i.fk_product AND m.fk_productvariant IS NOT DISTINCT FROM i.fk_productvariant
-- WHERE i.quantityonhand <> m.net;
-- SELECT invoicenumber, status, subtotal, taxamount, totalamount FROM purchases WHERE invoicenumber LIKE 'GB-DEMO-PO-%';
-- SELECT invoicenumber, subtotal, taxamount, totalamount, paidamount FROM sales WHERE invoicenumber LIKE 'GB-DEMO-S-%';
-- SELECT ordernumber, status, cancelled FROM online_orders WHERE ordernumber LIKE 'GB-DEMO-O-%' ORDER BY ordernumber;
-- SELECT u.username, r.rolecode FROM users u JOIN user_roles ur ON ur.fk_user = u.id_user JOIN roles r ON r.id_role = ur.fk_role WHERE u.username LIKE 'gb.%';

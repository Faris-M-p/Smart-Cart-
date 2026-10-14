-- =============================================================================
-- SmartCartPro | V001__InitialSchema.sql
-- Complete initial schema in dependency order.
-- Applied by ApplyPatches.bat / recorded in patch_history.
-- =============================================================================
\set ON_ERROR_STOP on

-- >>> 001_Company.sql

CREATE TABLE IF NOT EXISTS company (
    id_company      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    companycode     TEXT NOT NULL,
    companyname     TEXT NOT NULL,
    legalname       TEXT NULL,
    taxnumber       TEXT NULL,
    registrationno  TEXT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    website         TEXT NULL,
    addressline1    TEXT NULL,
    addressline2    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    currencycode    CHAR(3) NOT NULL DEFAULT 'INR',
    timezone        TEXT NOT NULL DEFAULT 'Asia/Kolkata',
    locale          TEXT NOT NULL DEFAULT 'en-IN',
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_company_companycode UNIQUE (companycode)
);

CREATE INDEX IF NOT EXISTS ix_company_isactive ON company (isactive) WHERE cancelled = FALSE;

-- >>> 002_Store.sql

-- Invariants (SQL + application):
-- 1. Store codes unique within company (uq_store_company_storecode).
-- 2. At most one default store per company (uq_store_one_default).
-- 3. Exactly one default store after initial setup: enforced in app transaction
--    that creates company + first store with isdefault=TRUE in one transaction.
-- 4. Do not deactivate/cancel the default store unless another active store is
--    first promoted to default (application transaction; not a trigger).
-- 5. Prevent multiple defaults via partial unique index below.

CREATE TABLE IF NOT EXISTS store (
    id_store        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_company      INT NOT NULL,
    storecode       TEXT NOT NULL,
    storename       TEXT NOT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    addressline1    TEXT NULL,
    addressline2    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_store_company
        FOREIGN KEY (fk_company) REFERENCES company (id_company),
    CONSTRAINT uq_store_company_storecode UNIQUE (fk_company, storecode),
    CONSTRAINT ck_store_default_active
        CHECK (isdefault = FALSE OR (isactive = TRUE AND cancelled = FALSE))
);

-- Exactly one default among non-cancelled stores per company (at most one).
CREATE UNIQUE INDEX IF NOT EXISTS uq_store_one_default
    ON store (fk_company)
    WHERE isdefault = TRUE AND cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_store_fk_company ON store (fk_company);
CREATE INDEX IF NOT EXISTS ix_store_isactive ON store (isactive) WHERE cancelled = FALSE;

-- >>> 003_Users.sql

CREATE TABLE IF NOT EXISTS users (
    id_user         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username        TEXT NOT NULL,
    email           TEXT NOT NULL,
    passwordhash    TEXT NOT NULL,
    fullname        TEXT NOT NULL,
    phone           TEXT NULL,
    employeecode    TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    mustchangepassword BOOLEAN NOT NULL DEFAULT TRUE,
    lastloginat     TIMESTAMP NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_users_isactive ON users (isactive) WHERE cancelled = FALSE;

-- >>> 004_Roles.sql

CREATE TABLE IF NOT EXISTS roles (
    id_role         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    rolecode        TEXT NOT NULL,
    rolename        TEXT NOT NULL,
    description     TEXT NULL,
    issystem        BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_roles_rolecode UNIQUE (rolecode)
);

-- >>> 005_Modules.sql

CREATE TABLE IF NOT EXISTS modules (
    id_module       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    modulecode      TEXT NOT NULL,
    modulename      TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_modules_modulecode UNIQUE (modulecode)
);

-- >>> 006_Permissions.sql

CREATE TABLE IF NOT EXISTS permissions (
    id_permission   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_module       INT NOT NULL,
    permissioncode  TEXT NOT NULL,
    permissionname  TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_permissions_permissioncode UNIQUE (permissioncode),
    CONSTRAINT fk_permissions_module
        FOREIGN KEY (fk_module) REFERENCES modules (id_module)
);

CREATE INDEX IF NOT EXISTS ix_permissions_fk_module ON permissions (fk_module);

-- >>> 007_RolePermissions.sql

CREATE TABLE IF NOT EXISTS role_permissions (
    id_rolepermission INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_role         INT NOT NULL,
    fk_permission   INT NOT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT uq_role_permissions UNIQUE (fk_role, fk_permission),
    CONSTRAINT fk_role_permissions_role
        FOREIGN KEY (fk_role) REFERENCES roles (id_role),
    CONSTRAINT fk_role_permissions_permission
        FOREIGN KEY (fk_permission) REFERENCES permissions (id_permission)
);

CREATE INDEX IF NOT EXISTS ix_role_permissions_fk_role ON role_permissions (fk_role);
CREATE INDEX IF NOT EXISTS ix_role_permissions_fk_permission ON role_permissions (fk_permission);

-- >>> 008_UserRoles.sql

CREATE TABLE IF NOT EXISTS user_roles (
    id_userrole     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user         INT NOT NULL,
    fk_role         INT NOT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT uq_user_roles UNIQUE (fk_user, fk_role),
    CONSTRAINT fk_user_roles_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT fk_user_roles_role
        FOREIGN KEY (fk_role) REFERENCES roles (id_role)
);

CREATE INDEX IF NOT EXISTS ix_user_roles_fk_user ON user_roles (fk_user);
CREATE INDEX IF NOT EXISTS ix_user_roles_fk_role ON user_roles (fk_role);

-- >>> 009_UserStoreAccess.sql

CREATE TABLE IF NOT EXISTS user_store_access (
    id_userstoreaccess INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user         INT NOT NULL,
    fk_store        INT NOT NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_user_store_access UNIQUE (fk_user, fk_store),
    CONSTRAINT fk_user_store_access_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT fk_user_store_access_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_user_store_access_one_default
    ON user_store_access (fk_user)
    WHERE isdefault = TRUE AND cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_user_store_access_fk_store ON user_store_access (fk_store);

-- >>> 010_Units.sql

CREATE TABLE IF NOT EXISTS units (
    id_unit         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    unitcode        TEXT NOT NULL,
    unitname        TEXT NOT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_units_unitcode UNIQUE (unitcode)
);

-- >>> 011_Taxes.sql

CREATE TABLE IF NOT EXISTS taxes (
    id_tax          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    taxcode         TEXT NOT NULL,
    taxname         TEXT NOT NULL,
    taxrate         NUMERIC(7,4) NOT NULL DEFAULT 0,
    isinclusive     BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_taxes_taxcode UNIQUE (taxcode),
    CONSTRAINT ck_taxes_taxrate CHECK (taxrate >= 0)
);

-- >>> 012_Categories.sql

CREATE TABLE IF NOT EXISTS categories (
    id_category     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categorycode    TEXT NULL,
    categoryname    TEXT NOT NULL,
    description     TEXT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_categories_categoryname UNIQUE (categoryname)
);

-- >>> 013_SubCategories.sql

CREATE TABLE IF NOT EXISTS subcategories (
    id_subcategory  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_category     INT NOT NULL,
    subcategoryname TEXT NOT NULL,
    description     TEXT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_subcategories_category_name UNIQUE (fk_category, subcategoryname),
    CONSTRAINT fk_subcategories_category
        FOREIGN KEY (fk_category) REFERENCES categories (id_category)
);

CREATE INDEX IF NOT EXISTS ix_subcategories_fk_category ON subcategories (fk_category);

-- >>> 014_Brands.sql

CREATE TABLE IF NOT EXISTS brands (
    id_brand        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    brandname       TEXT NOT NULL,
    description     TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_brands_brandname UNIQUE (brandname)
);

-- >>> 015_Products.sql

CREATE TABLE IF NOT EXISTS products (
    id_product      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_category     INT NULL,
    fk_subcategory  INT NULL,
    fk_brand        INT NULL,
    fk_unit         INT NULL,
    fk_tax          INT NULL,
    productcode     TEXT NOT NULL,
    productname     TEXT NOT NULL,
    description     TEXT NULL,
    sku             TEXT NULL,
    barcode         TEXT NULL,
    costprice       NUMERIC(12,2) NOT NULL DEFAULT 0,
    sellingprice    NUMERIC(12,2) NOT NULL DEFAULT 0,
    mrp             NUMERIC(12,2) NULL,
    reorderlevel    NUMERIC(12,3) NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    isonline        BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_products_productcode UNIQUE (productcode),
    CONSTRAINT uq_products_sku UNIQUE (sku),
    CONSTRAINT uq_products_barcode UNIQUE (barcode),
    CONSTRAINT fk_products_category
        FOREIGN KEY (fk_category) REFERENCES categories (id_category),
    CONSTRAINT fk_products_subcategory
        FOREIGN KEY (fk_subcategory) REFERENCES subcategories (id_subcategory),
    CONSTRAINT fk_products_brand
        FOREIGN KEY (fk_brand) REFERENCES brands (id_brand),
    CONSTRAINT fk_products_unit
        FOREIGN KEY (fk_unit) REFERENCES units (id_unit),
    CONSTRAINT fk_products_tax
        FOREIGN KEY (fk_tax) REFERENCES taxes (id_tax),
    CONSTRAINT ck_products_prices CHECK (costprice >= 0 AND sellingprice >= 0)
);

CREATE INDEX IF NOT EXISTS ix_products_fk_category ON products (fk_category);
CREATE INDEX IF NOT EXISTS ix_products_fk_brand ON products (fk_brand);
CREATE INDEX IF NOT EXISTS ix_products_isonline ON products (isonline) WHERE cancelled = FALSE;

-- >>> 016_ProductVariants.sql

CREATE TABLE IF NOT EXISTS product_variants (
    id_productvariant INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    variantname     TEXT NOT NULL,
    skucode         TEXT NULL,
    barcode         TEXT NULL,
    additionalprice NUMERIC(12,2) NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_product_variants_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT uq_product_variants_product_name UNIQUE (fk_product, variantname),
    CONSTRAINT uq_product_variants_skucode UNIQUE (skucode),
    CONSTRAINT uq_product_variants_barcode UNIQUE (barcode)
);

CREATE INDEX IF NOT EXISTS ix_product_variants_fk_product ON product_variants (fk_product);

-- >>> 017_ProductImages.sql

CREATE TABLE IF NOT EXISTS product_images (
    id_productimage INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    imagepath       TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isprimary       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_product_images_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product)
);

CREATE INDEX IF NOT EXISTS ix_product_images_fk_product ON product_images (fk_product);

-- >>> 018_Suppliers.sql

CREATE TABLE IF NOT EXISTS suppliers (
    id_supplier     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    suppliercode    TEXT NULL,
    suppliername    TEXT NOT NULL,
    contactperson   TEXT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    addressline1    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    taxnumber       TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_suppliers_suppliername UNIQUE (suppliername)
);

-- >>> 019_Customers.sql

CREATE TABLE IF NOT EXISTS customers (
    id_customer     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customercode    TEXT NULL,
    fullname        TEXT NOT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    passwordhash    TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_customers_email UNIQUE (email),
    CONSTRAINT uq_customers_phone UNIQUE (phone)
);

CREATE INDEX IF NOT EXISTS ix_customers_fullname ON customers (fullname);

-- >>> 020_CustomerAddresses.sql

CREATE TABLE IF NOT EXISTS customer_addresses (
    id_customeraddress INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_customer     INT NOT NULL,
    addresstype     TEXT NOT NULL DEFAULT 'shipping',
    addressline1    TEXT NOT NULL,
    addressline2    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_customer_addresses_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer)
);

CREATE INDEX IF NOT EXISTS ix_customer_addresses_fk_customer ON customer_addresses (fk_customer);

-- >>> 021_Inventory.sql

CREATE TABLE IF NOT EXISTS inventory (
    id_inventory    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantityonhand  NUMERIC(14,3) NOT NULL DEFAULT 0,
    quantityreserved NUMERIC(14,3) NOT NULL DEFAULT 0,
    reorderlevel    NUMERIC(14,3) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_inventory_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_inventory_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_inventory_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_inventory_qty CHECK (quantityonhand >= 0 AND quantityreserved >= 0)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_inventory_store_product_novariant
    ON inventory (fk_store, fk_product)
    WHERE fk_productvariant IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_inventory_store_product_variant
    ON inventory (fk_store, fk_product, fk_productvariant)
    WHERE fk_productvariant IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_inventory_fk_store ON inventory (fk_store);
CREATE INDEX IF NOT EXISTS ix_inventory_fk_product ON inventory (fk_product);

-- >>> 022_StockMovements.sql

CREATE TABLE IF NOT EXISTS stock_movements (
    id_stockmovement INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    movementtype    TEXT NOT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    referencetype   TEXT NULL,
    referenceid     INT NULL,
    notes           TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    createdby       INT NULL,
    CONSTRAINT fk_stock_movements_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_stock_movements_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_stock_movements_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT fk_stock_movements_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT ck_stock_movements_type
        CHECK (movementtype IN ('purchase','sale','adjustment','transfer_in','transfer_out','return_in','return_out','opening'))
);

CREATE INDEX IF NOT EXISTS ix_stock_movements_fk_store ON stock_movements (fk_store);
CREATE INDEX IF NOT EXISTS ix_stock_movements_fk_product ON stock_movements (fk_product);
CREATE INDEX IF NOT EXISTS ix_stock_movements_createdat ON stock_movements (createdat);

-- >>> 023_Purchases.sql

CREATE TABLE IF NOT EXISTS purchases (
    id_purchase     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_supplier     INT NOT NULL,
    purchasedate    DATE NOT NULL DEFAULT CURRENT_DATE,
    invoicenumber   TEXT NULL,
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    notes           TEXT NULL,
    status          TEXT NOT NULL DEFAULT 'posted',
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_purchases_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_purchases_supplier
        FOREIGN KEY (fk_supplier) REFERENCES suppliers (id_supplier),
    CONSTRAINT fk_purchases_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user)
);

CREATE INDEX IF NOT EXISTS ix_purchases_fk_store ON purchases (fk_store);
CREATE INDEX IF NOT EXISTS ix_purchases_fk_supplier ON purchases (fk_supplier);
CREATE INDEX IF NOT EXISTS ix_purchases_purchasedate ON purchases (purchasedate);

-- >>> 024_PurchaseDetails.sql

CREATE TABLE IF NOT EXISTS purchase_details (
    id_purchasedetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchase     INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitcost        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_purchase_details_purchase
        FOREIGN KEY (fk_purchase) REFERENCES purchases (id_purchase),
    CONSTRAINT fk_purchase_details_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_purchase_details_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_purchase_details_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_purchase_details_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_purchase_details_fk_purchase ON purchase_details (fk_purchase);
CREATE INDEX IF NOT EXISTS ix_purchase_details_fk_store ON purchase_details (fk_store);

-- >>> 027_CashSessions.sql

CREATE TABLE IF NOT EXISTS cash_sessions (
    id_cashsession  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_user         INT NOT NULL,
    openedat        TIMESTAMP NOT NULL DEFAULT NOW(),
    closedat        TIMESTAMP NULL,
    openingfloat    NUMERIC(12,2) NOT NULL DEFAULT 0,
    closingcash     NUMERIC(12,2) NULL,
    expectedcash    NUMERIC(12,2) NULL,
    variance        NUMERIC(12,2) NULL,
    status          TEXT NOT NULL DEFAULT 'open',
    notes           TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_cash_sessions_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_cash_sessions_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT ck_cash_sessions_status CHECK (status IN ('open','closed'))
);

CREATE INDEX IF NOT EXISTS ix_cash_sessions_fk_store ON cash_sessions (fk_store);
CREATE INDEX IF NOT EXISTS ix_cash_sessions_status ON cash_sessions (status);

-- >>> 025_Sales.sql

CREATE TABLE IF NOT EXISTS sales (
    id_sale         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NULL,
    fk_cashsession  INT NULL,
    invoicenumber   TEXT NOT NULL,
    saledate        TIMESTAMP NOT NULL DEFAULT NOW(),
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    paidamount      NUMERIC(12,2) NOT NULL DEFAULT 0,
    changegiven     NUMERIC(12,2) NOT NULL DEFAULT 0,
    status          TEXT NOT NULL DEFAULT 'completed',
    notes           TEXT NULL,
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_sales_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sales_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT fk_sales_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT uq_sales_store_invoicenumber UNIQUE (fk_store, invoicenumber)
);

CREATE INDEX IF NOT EXISTS ix_sales_fk_store ON sales (fk_store);
CREATE INDEX IF NOT EXISTS ix_sales_saledate ON sales (saledate);
CREATE INDEX IF NOT EXISTS ix_sales_fk_customer ON sales (fk_customer);

-- >>> 026_SaleDetails.sql

CREATE TABLE IF NOT EXISTS sale_details (
    id_saledetail   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_sale         INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitprice       NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_sale_details_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_sale_details_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sale_details_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_sale_details_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_sale_details_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_sale_details_fk_sale ON sale_details (fk_sale);
CREATE INDEX IF NOT EXISTS ix_sale_details_fk_store ON sale_details (fk_store);

-- >>> 028_Payments.sql

CREATE TABLE IF NOT EXISTS payments (
    id_payment      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_sale         INT NULL,
    fk_cashsession  INT NULL,
    paymentmethod   TEXT NOT NULL,
    amount          NUMERIC(12,2) NOT NULL,
    referenceno     TEXT NULL,
    paidat          TIMESTAMP NOT NULL DEFAULT NOW(),
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_payments_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_payments_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_payments_cashsession
        FOREIGN KEY (fk_cashsession) REFERENCES cash_sessions (id_cashsession),
    CONSTRAINT fk_payments_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT ck_payments_amount CHECK (amount > 0),
    CONSTRAINT ck_payments_method
        CHECK (paymentmethod IN ('cash','card','upi','wallet','bank','other'))
);

CREATE INDEX IF NOT EXISTS ix_payments_fk_store ON payments (fk_store);
CREATE INDEX IF NOT EXISTS ix_payments_fk_sale ON payments (fk_sale);

-- >>> 029_SalesReturns.sql

CREATE TABLE IF NOT EXISTS sales_returns (
    id_salesreturn  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_sale         INT NOT NULL,
    returnnumber    TEXT NOT NULL,
    returndate      TIMESTAMP NOT NULL DEFAULT NOW(),
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    reason          TEXT NULL,
    status          TEXT NOT NULL DEFAULT 'completed',
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_sales_returns_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sales_returns_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_sales_returns_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT uq_sales_returns_store_number UNIQUE (fk_store, returnnumber)
);

CREATE INDEX IF NOT EXISTS ix_sales_returns_fk_store ON sales_returns (fk_store);
CREATE INDEX IF NOT EXISTS ix_sales_returns_fk_sale ON sales_returns (fk_sale);

-- >>> 030_SalesReturnDetails.sql

CREATE TABLE IF NOT EXISTS sales_return_details (
    id_salesreturndetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_salesreturn  INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitprice       NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_sales_return_details_return
        FOREIGN KEY (fk_salesreturn) REFERENCES sales_returns (id_salesreturn),
    CONSTRAINT fk_sales_return_details_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sales_return_details_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_sales_return_details_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_sales_return_details_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_sales_return_details_fk_return ON sales_return_details (fk_salesreturn);
CREATE INDEX IF NOT EXISTS ix_sales_return_details_fk_store ON sales_return_details (fk_store);

-- >>> 031_Expenses.sql

CREATE TABLE IF NOT EXISTS expenses (
    id_expense      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    expensedate     DATE NOT NULL DEFAULT CURRENT_DATE,
    category        TEXT NOT NULL,
    description     TEXT NULL,
    amount          NUMERIC(12,2) NOT NULL,
    paymentmethod   TEXT NULL,
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_expenses_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_expenses_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT ck_expenses_amount CHECK (amount >= 0)
);

CREATE INDEX IF NOT EXISTS ix_expenses_fk_store ON expenses (fk_store);
CREATE INDEX IF NOT EXISTS ix_expenses_expensedate ON expenses (expensedate);

-- >>> 032_Settings.sql

-- setting_scope: 'platform' | 'company' | 'store'
-- When scope = 'store', fk_store is required (application + CHECK).
CREATE TABLE IF NOT EXISTS settings (
    id_setting      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    setting_scope   TEXT NOT NULL DEFAULT 'company',
    fk_store        INT NULL,
    settingkey      TEXT NOT NULL,
    settingvalue    TEXT NULL,
    valuetype       TEXT NOT NULL DEFAULT 'string',
    description     TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_settings_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT ck_settings_scope
        CHECK (setting_scope IN ('platform','company','store')),
    CONSTRAINT ck_settings_store_scope
        CHECK (
            (setting_scope = 'store' AND fk_store IS NOT NULL)
            OR (setting_scope <> 'store' AND fk_store IS NULL)
        ),
    CONSTRAINT uq_settings_scope_store_key UNIQUE (setting_scope, fk_store, settingkey)
);

CREATE INDEX IF NOT EXISTS ix_settings_fk_store ON settings (fk_store);

-- >>> 033_AuditLogs.sql

CREATE TABLE IF NOT EXISTS audit_logs (
    id_auditlog     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NULL,
    fk_user         INT NULL,
    entityname      TEXT NOT NULL,
    entityid        INT NULL,
    action          TEXT NOT NULL,
    details         TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_audit_logs_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_audit_logs_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

CREATE INDEX IF NOT EXISTS ix_audit_logs_createdat ON audit_logs (createdat);
CREATE INDEX IF NOT EXISTS ix_audit_logs_entity ON audit_logs (entityname, entityid);

-- >>> 034_OnlineOrders.sql

CREATE TABLE IF NOT EXISTS online_orders (
    id_onlineorder  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NOT NULL,
    ordernumber     TEXT NOT NULL,
    orderdate       TIMESTAMP NOT NULL DEFAULT NOW(),
    status          TEXT NOT NULL DEFAULT 'pending',
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    shippingamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    shippingaddress TEXT NULL,
    notes           TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_online_orders_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_online_orders_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT uq_online_orders_store_number UNIQUE (fk_store, ordernumber)
);

CREATE INDEX IF NOT EXISTS ix_online_orders_fk_store ON online_orders (fk_store);
CREATE INDEX IF NOT EXISTS ix_online_orders_fk_customer ON online_orders (fk_customer);
CREATE INDEX IF NOT EXISTS ix_online_orders_status ON online_orders (status);

-- >>> 035_OnlineOrderItems.sql

CREATE TABLE IF NOT EXISTS online_order_items (
    id_onlineorderitem INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_onlineorder  INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL,
    unitprice       NUMERIC(12,2) NOT NULL DEFAULT 0,
    linetotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_online_order_items_order
        FOREIGN KEY (fk_onlineorder) REFERENCES online_orders (id_onlineorder),
    CONSTRAINT fk_online_order_items_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_online_order_items_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_online_order_items_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT ck_online_order_items_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_online_order_items_fk_order ON online_order_items (fk_onlineorder);
CREATE INDEX IF NOT EXISTS ix_online_order_items_fk_store ON online_order_items (fk_store);

-- >>> 036_Carts.sql

CREATE TABLE IF NOT EXISTS carts (
    id_cart         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NULL,
    sessionkey      TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_carts_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_carts_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer)
);

CREATE INDEX IF NOT EXISTS ix_carts_fk_store ON carts (fk_store);
CREATE INDEX IF NOT EXISTS ix_carts_fk_customer ON carts (fk_customer);

-- >>> 037_CartItems.sql

CREATE TABLE IF NOT EXISTS cart_items (
    id_cartitem     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_cart         INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    fk_productvariant INT NULL,
    quantity        NUMERIC(14,3) NOT NULL DEFAULT 1,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_cart_items_cart
        FOREIGN KEY (fk_cart) REFERENCES carts (id_cart) ON DELETE CASCADE,
    CONSTRAINT fk_cart_items_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_cart_items_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_cart_items_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES product_variants (id_productvariant),
    CONSTRAINT uq_cart_items UNIQUE (fk_cart, fk_product, fk_productvariant),
    CONSTRAINT ck_cart_items_qty CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_cart_items_fk_store ON cart_items (fk_store);

-- >>> 038_Wishlists.sql

CREATE TABLE IF NOT EXISTS wishlists (
    id_wishlist     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NOT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_wishlists_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_wishlists_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT uq_wishlists_store_customer UNIQUE (fk_store, fk_customer)
);

CREATE INDEX IF NOT EXISTS ix_wishlists_fk_store ON wishlists (fk_store);

-- >>> 039_WishlistItems.sql

CREATE TABLE IF NOT EXISTS wishlist_items (
    id_wishlistitem INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_wishlist     INT NOT NULL,
    fk_store        INT NOT NULL,
    fk_product      INT NOT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_wishlist_items_wishlist
        FOREIGN KEY (fk_wishlist) REFERENCES wishlists (id_wishlist) ON DELETE CASCADE,
    CONSTRAINT fk_wishlist_items_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_wishlist_items_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT uq_wishlist_items UNIQUE (fk_wishlist, fk_product)
);

CREATE INDEX IF NOT EXISTS ix_wishlist_items_fk_store ON wishlist_items (fk_store);

-- >>> 040_ProductReviews.sql

CREATE TABLE IF NOT EXISTS product_reviews (
    id_productreview INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    fk_customer     INT NOT NULL,
    fk_store        INT NULL,
    fk_onlineorder  INT NULL,
    rating          INT NOT NULL,
    title           TEXT NULL,
    reviewtext      TEXT NULL,
    isapproved      BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_product_reviews_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_product_reviews_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT fk_product_reviews_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_product_reviews_onlineorder
        FOREIGN KEY (fk_onlineorder) REFERENCES online_orders (id_onlineorder),
    CONSTRAINT ck_product_reviews_rating CHECK (rating BETWEEN 1 AND 5)
);

CREATE INDEX IF NOT EXISTS ix_product_reviews_fk_product ON product_reviews (fk_product);

-- >>> 041_InvoiceSequences.sql

CREATE TABLE IF NOT EXISTS invoice_sequences (
    id_invoicesequence INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    sequencetype    TEXT NOT NULL,
    prefix          TEXT NOT NULL DEFAULT '',
    nextnumber      INT NOT NULL DEFAULT 1,
    padding         INT NOT NULL DEFAULT 6,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_invoice_sequences_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT uq_invoice_sequences UNIQUE (fk_store, sequencetype),
    CONSTRAINT ck_invoice_sequences_next CHECK (nextnumber >= 1)
);

CREATE INDEX IF NOT EXISTS ix_invoice_sequences_fk_store ON invoice_sequences (fk_store);

-- >>> 042_PatchHistory.sql

CREATE TABLE IF NOT EXISTS patch_history (
    id_patchhistory INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    version         TEXT NOT NULL,
    description     TEXT NULL,
    appliedat       TIMESTAMP NOT NULL DEFAULT NOW(),
    appliedby       TEXT NULL,
    success         BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_patch_history_version UNIQUE (version)
);


-- Wire sales.fk_cashsession after cash_sessions exists
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_sales_cashsession'
    ) THEN
        ALTER TABLE sales
            ADD CONSTRAINT fk_sales_cashsession
            FOREIGN KEY (fk_cashsession) REFERENCES cash_sessions (id_cashsession);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_sales_fk_cashsession ON sales (fk_cashsession);

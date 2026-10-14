-- =============================================================================
-- SmartCartPro | 015_Products.sql
-- Scope: Company-wide (shared across stores)
-- =============================================================================

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

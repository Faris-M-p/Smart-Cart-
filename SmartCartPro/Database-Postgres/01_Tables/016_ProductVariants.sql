-- =============================================================================
-- SmartCartPro | 016_ProductVariants.sql
-- Scope: Company-wide
-- =============================================================================

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

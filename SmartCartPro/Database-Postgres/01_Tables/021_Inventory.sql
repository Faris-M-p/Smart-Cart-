-- =============================================================================
-- SmartCartPro | 021_Inventory.sql
-- Scope: Store-scoped
-- =============================================================================

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

-- PostgreSQL UNIQUE allows multiple NULLs; use partial indexes instead.
CREATE UNIQUE INDEX IF NOT EXISTS uq_inventory_store_product_novariant
    ON inventory (fk_store, fk_product)
    WHERE fk_productvariant IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_inventory_store_product_variant
    ON inventory (fk_store, fk_product, fk_productvariant)
    WHERE fk_productvariant IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_inventory_fk_store ON inventory (fk_store);
CREATE INDEX IF NOT EXISTS ix_inventory_fk_product ON inventory (fk_product);

-- =============================================================================
-- SmartCartPro | 035_OnlineOrderItems.sql
-- Scope: Store-scoped
-- =============================================================================

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

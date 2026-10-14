-- =============================================================================
-- SmartCartPro | 037_CartItems.sql
-- Scope: Store-scoped
-- =============================================================================

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

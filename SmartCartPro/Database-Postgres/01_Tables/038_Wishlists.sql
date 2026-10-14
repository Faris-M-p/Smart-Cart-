-- =============================================================================
-- SmartCartPro | 038_Wishlists.sql
-- Scope: Store-scoped
-- =============================================================================

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

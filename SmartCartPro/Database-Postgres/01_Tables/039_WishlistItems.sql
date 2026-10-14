-- =============================================================================
-- SmartCartPro | 039_WishlistItems.sql
-- Scope: Store-scoped
-- =============================================================================

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

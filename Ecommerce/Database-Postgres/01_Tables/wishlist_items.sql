CREATE TABLE IF NOT EXISTS wishlist_items (
    wishlist_item_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    wishlist_id      INT NOT NULL,
    product_id       INT NOT NULL,
    created_at       TIMESTAMP NULL DEFAULT NOW(),
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT fk_wishlist_items_wishlist
        FOREIGN KEY (wishlist_id) REFERENCES wishlist (wishlist_id),
    CONSTRAINT fk_wishlist_items_product
        FOREIGN KEY (product_id) REFERENCES products (id_product)
);

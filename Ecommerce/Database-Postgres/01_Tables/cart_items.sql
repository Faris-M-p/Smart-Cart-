CREATE TABLE IF NOT EXISTS cart_items (
    cart_item_id       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cart_id            INT NOT NULL,
    product_id         INT NOT NULL,
    product_variant_id INT NULL,
    quantity           INT NOT NULL,
    price              NUMERIC(10,2) NOT NULL,
    created_at         TIMESTAMP NULL DEFAULT NOW(),
    CONSTRAINT fk_cart_items_cart
        FOREIGN KEY (cart_id) REFERENCES cart (cart_id),
    CONSTRAINT fk_cart_items_product
        FOREIGN KEY (product_id) REFERENCES products (id_product),
    CONSTRAINT fk_cart_items_product_variant
        FOREIGN KEY (product_variant_id) REFERENCES product_variants (id_product_variant)
);

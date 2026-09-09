CREATE TABLE IF NOT EXISTS order_items (
    id_order_item      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_order           INT NOT NULL,
    fk_product         INT NOT NULL,
    fk_product_variant INT NULL,
    product_name       TEXT NOT NULL,
    variant_label      TEXT NULL,
    sku                TEXT NULL,
    unit_price         NUMERIC(10,2) NOT NULL,
    quantity           INT NOT NULL,
    line_total         NUMERIC(10,2) NOT NULL,
    created_at         TIMESTAMP NULL DEFAULT NOW(),
    CONSTRAINT fk_order_items_order
        FOREIGN KEY (fk_order) REFERENCES orders (order_id),
    CONSTRAINT fk_order_items_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_order_items_product_variant
        FOREIGN KEY (fk_product_variant) REFERENCES product_variants (id_product_variant)
);

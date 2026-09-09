CREATE TABLE IF NOT EXISTS product_variants (
    id_product_variant INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product         INT NOT NULL,
    sku                TEXT NOT NULL,
    barcode            TEXT NULL,
    variant_label      TEXT NOT NULL,
    description        TEXT NULL,
    mrp                NUMERIC(10,2) NOT NULL,
    selling_price      NUMERIC(10,2) NOT NULL,
    unit_of_measure    TEXT NULL,
    unit_value         NUMERIC(10,3) NULL,
    is_default         BOOLEAN DEFAULT FALSE,
    max_order_qty      INT DEFAULT 10,
    is_active          BOOLEAN DEFAULT TRUE,
    sell_online        BOOLEAN NOT NULL DEFAULT FALSE,
    created_at         TIMESTAMP DEFAULT NOW(),
    cancelled          BOOLEAN DEFAULT FALSE,
    cancelled_on       TIMESTAMP NULL,
    CONSTRAINT uq_product_variants_sku UNIQUE (sku),
    CONSTRAINT uq_product_variants_barcode UNIQUE (barcode),
    CONSTRAINT fk_product_variants_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product)
);

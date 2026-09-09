CREATE TABLE IF NOT EXISTS stock (
    id_stock           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchase_detail INT NOT NULL,
    fk_product_variant INT NOT NULL,
    quantity           INT NOT NULL,
    created_on         TIMESTAMP NOT NULL DEFAULT NOW(),
    enter_by           INT NULL,
    cancelled          BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on       TIMESTAMP NULL,
    cancelled_reason   TEXT NULL,
    cancelled_by       INT NULL,
    CONSTRAINT fk_stock_purchase_detail
        FOREIGN KEY (fk_purchase_detail) REFERENCES purchase_detail (id_purchase_detail),
    CONSTRAINT fk_stock_product_variant
        FOREIGN KEY (fk_product_variant) REFERENCES product_variants (id_product_variant)
);

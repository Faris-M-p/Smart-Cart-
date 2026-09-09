CREATE TABLE IF NOT EXISTS purchase_detail (
    id_purchase_detail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchase        INT NOT NULL,
    fk_product_variant INT NOT NULL,
    quantity           INT NOT NULL,
    purchase_price     NUMERIC(18,2) NOT NULL DEFAULT 0,
    mrp                NUMERIC(18,2) NULL,
    expiry_date        DATE NULL,
    created_on         TIMESTAMP NOT NULL DEFAULT NOW(),
    enter_by           INT NULL,
    cancelled          BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on       TIMESTAMP NULL,
    cancelled_reason   TEXT NULL,
    cancelled_by       INT NULL,
    CONSTRAINT fk_purchase_detail_purchase
        FOREIGN KEY (fk_purchase) REFERENCES purchase (id_purchase),
    CONSTRAINT fk_purchase_detail_product_variant
        FOREIGN KEY (fk_product_variant) REFERENCES product_variants (id_product_variant)
);

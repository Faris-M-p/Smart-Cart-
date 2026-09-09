CREATE TABLE IF NOT EXISTS sku_media (
    id_sku_media   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product_sku INT NOT NULL,
    media_type     TEXT NOT NULL DEFAULT 'Image',
    media_url      TEXT NOT NULL,
    display_order  INT NOT NULL DEFAULT 0,
    is_primary     BOOLEAN NOT NULL DEFAULT FALSE,
    created_at     TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMP NULL,
    CONSTRAINT fk_sku_media_product_variant
        FOREIGN KEY (fk_product_sku) REFERENCES product_variants (id_product_variant),
    CONSTRAINT ck_sku_media_media_type
        CHECK (media_type = 'Image')
);

CREATE INDEX IF NOT EXISTS ix_sku_media_fk_product_sku
    ON sku_media (fk_product_sku, display_order);

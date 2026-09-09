CREATE TABLE IF NOT EXISTS product_variant_images (
    id_product_variant_image INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product_variant       INT NOT NULL,
    image_url                TEXT NOT NULL,
    is_primary               BOOLEAN NOT NULL DEFAULT FALSE,
    display_order            INT NOT NULL DEFAULT 0,
    created_at               TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_product_variant_images_product_variant
        FOREIGN KEY (fk_product_variant) REFERENCES product_variants (id_product_variant)
);

CREATE INDEX IF NOT EXISTS ix_product_variant_images_fk_product_variant
    ON product_variant_images (fk_product_variant);

CREATE TABLE IF NOT EXISTS product_media (
    id_product_media INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product       INT NOT NULL,
    media_type       TEXT NOT NULL,
    media_url        TEXT NOT NULL,
    display_order    INT NOT NULL DEFAULT 0,
    is_primary       BOOLEAN NOT NULL DEFAULT FALSE,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    CONSTRAINT fk_product_media_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT ck_product_media_media_type
        CHECK (media_type IN ('Image', 'Video'))
);

CREATE INDEX IF NOT EXISTS ix_product_media_fk_product
    ON product_media (fk_product, display_order);

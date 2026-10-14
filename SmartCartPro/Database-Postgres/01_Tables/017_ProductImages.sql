-- =============================================================================
-- SmartCartPro | 017_ProductImages.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS product_images (
    id_productimage INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    imagepath       TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isprimary       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_product_images_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product)
);

CREATE INDEX IF NOT EXISTS ix_product_images_fk_product ON product_images (fk_product);

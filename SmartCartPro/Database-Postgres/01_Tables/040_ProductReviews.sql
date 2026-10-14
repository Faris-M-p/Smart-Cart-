-- =============================================================================
-- SmartCartPro | 040_ProductReviews.sql
-- Scope: Company-wide (optional store context)
-- =============================================================================

CREATE TABLE IF NOT EXISTS product_reviews (
    id_productreview INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    fk_customer     INT NOT NULL,
    fk_store        INT NULL,
    fk_onlineorder  INT NULL,
    rating          INT NOT NULL,
    title           TEXT NULL,
    reviewtext      TEXT NULL,
    isapproved      BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_product_reviews_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_product_reviews_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT fk_product_reviews_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_product_reviews_onlineorder
        FOREIGN KEY (fk_onlineorder) REFERENCES online_orders (id_onlineorder),
    CONSTRAINT ck_product_reviews_rating CHECK (rating BETWEEN 1 AND 5)
);

CREATE INDEX IF NOT EXISTS ix_product_reviews_fk_product ON product_reviews (fk_product);

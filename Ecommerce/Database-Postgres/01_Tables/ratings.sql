CREATE TABLE IF NOT EXISTS ratings (
    id_rating       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    fk_user         INT NOT NULL,
    ratingvalue     NUMERIC(2,1) NOT NULL,
    review          TEXT NULL,
    createdat       TIMESTAMP NULL DEFAULT NOW(),
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_ratings_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_ratings_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

/* -----------------------------------------------------------------------------
   Order-item based reviews (idempotent — safe on fresh builds and on existing DBs)

   A review now belongs to  Customer + Order + OrderItem + Product.
   - fk_order / fk_orderitem are NULLABLE on purpose: the original sample reviews
     were seeded without an order link. They stay as historical/sample product
     reviews (read-only for customers) — no fake order-item links are created.
   - One ACTIVE review per order item is enforced by a partial unique index.
     (Soft-deleted rows — cancelled = TRUE — do not block a new review.)
   ----------------------------------------------------------------------------- */
ALTER TABLE ratings ADD COLUMN IF NOT EXISTS fk_order     INT NULL;
ALTER TABLE ratings ADD COLUMN IF NOT EXISTS fk_orderitem INT NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_ratings_order') THEN
        ALTER TABLE ratings
            ADD CONSTRAINT fk_ratings_order
            FOREIGN KEY (fk_order) REFERENCES orders (id_order);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_ratings_orderitem') THEN
        ALTER TABLE ratings
            ADD CONSTRAINT fk_ratings_orderitem
            FOREIGN KEY (fk_orderitem) REFERENCES orderitems (id_orderitem);
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ux_ratings_orderitem_active
    ON ratings (fk_orderitem)
    WHERE fk_orderitem IS NOT NULL AND cancelled IS NOT TRUE;

CREATE INDEX IF NOT EXISTS ix_ratings_product_active
    ON ratings (fk_product)
    WHERE cancelled IS NOT TRUE;

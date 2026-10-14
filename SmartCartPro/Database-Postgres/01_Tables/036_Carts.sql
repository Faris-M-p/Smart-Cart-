-- =============================================================================
-- SmartCartPro | 036_Carts.sql
-- Scope: Store-scoped (storefront cart)
-- =============================================================================

CREATE TABLE IF NOT EXISTS carts (
    id_cart         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NULL,
    sessionkey      TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_carts_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_carts_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer)
);

CREATE INDEX IF NOT EXISTS ix_carts_fk_store ON carts (fk_store);
CREATE INDEX IF NOT EXISTS ix_carts_fk_customer ON carts (fk_customer);

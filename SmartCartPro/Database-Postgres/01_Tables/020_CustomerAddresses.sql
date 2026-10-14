-- =============================================================================
-- SmartCartPro | 020_CustomerAddresses.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS customer_addresses (
    id_customeraddress INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_customer     INT NOT NULL,
    addresstype     TEXT NOT NULL DEFAULT 'shipping',
    addressline1    TEXT NOT NULL,
    addressline2    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_customer_addresses_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer)
);

CREATE INDEX IF NOT EXISTS ix_customer_addresses_fk_customer ON customer_addresses (fk_customer);

-- =============================================================================
-- SmartCartPro | 019_Customers.sql
-- Scope: Company-wide (shared customer master)
-- =============================================================================

CREATE TABLE IF NOT EXISTS customers (
    id_customer     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customercode    TEXT NULL,
    fullname        TEXT NOT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    passwordhash    TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_customers_email UNIQUE (email),
    CONSTRAINT uq_customers_phone UNIQUE (phone)
);

CREATE INDEX IF NOT EXISTS ix_customers_fullname ON customers (fullname);

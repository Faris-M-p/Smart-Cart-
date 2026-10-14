-- =============================================================================
-- SmartCartPro | 018_Suppliers.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS suppliers (
    id_supplier     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    suppliercode    TEXT NULL,
    suppliername    TEXT NOT NULL,
    contactperson   TEXT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    addressline1    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    taxnumber       TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_suppliers_suppliername UNIQUE (suppliername)
);

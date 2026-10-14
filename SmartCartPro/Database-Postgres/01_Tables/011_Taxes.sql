-- =============================================================================
-- SmartCartPro | 011_Taxes.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS taxes (
    id_tax          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    taxcode         TEXT NOT NULL,
    taxname         TEXT NOT NULL,
    taxrate         NUMERIC(7,4) NOT NULL DEFAULT 0,
    isinclusive     BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_taxes_taxcode UNIQUE (taxcode),
    CONSTRAINT ck_taxes_taxrate CHECK (taxrate >= 0)
);

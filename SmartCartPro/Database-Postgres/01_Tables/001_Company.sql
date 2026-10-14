-- =============================================================================
-- SmartCartPro | 001_Company.sql
-- Scope: Platform-wide within client DB (one company per client database)
-- =============================================================================

CREATE TABLE IF NOT EXISTS company (
    id_company      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    companycode     TEXT NOT NULL,
    companyname     TEXT NOT NULL,
    legalname       TEXT NULL,
    taxnumber       TEXT NULL,
    registrationno  TEXT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    website         TEXT NULL,
    addressline1    TEXT NULL,
    addressline2    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    currencycode    CHAR(3) NOT NULL DEFAULT 'INR',
    timezone        TEXT NOT NULL DEFAULT 'Asia/Kolkata',
    locale          TEXT NOT NULL DEFAULT 'en-IN',
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_company_companycode UNIQUE (companycode)
);

CREATE INDEX IF NOT EXISTS ix_company_isactive ON company (isactive) WHERE cancelled = FALSE;

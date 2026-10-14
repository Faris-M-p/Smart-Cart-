-- =============================================================================
-- SmartCartPro | 014_Brands.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS brands (
    id_brand        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    brandname       TEXT NOT NULL,
    description     TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_brands_brandname UNIQUE (brandname)
);

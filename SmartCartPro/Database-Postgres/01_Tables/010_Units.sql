-- =============================================================================
-- SmartCartPro | 010_Units.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS units (
    id_unit         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    unitcode        TEXT NOT NULL,
    unitname        TEXT NOT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_units_unitcode UNIQUE (unitcode)
);

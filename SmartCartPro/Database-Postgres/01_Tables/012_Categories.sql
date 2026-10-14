-- =============================================================================
-- SmartCartPro | 012_Categories.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS categories (
    id_category     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categorycode    TEXT NULL,
    categoryname    TEXT NOT NULL,
    description     TEXT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_categories_categoryname UNIQUE (categoryname)
);

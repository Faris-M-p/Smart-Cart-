-- =============================================================================
-- SmartCartPro | 013_SubCategories.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS subcategories (
    id_subcategory  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_category     INT NOT NULL,
    subcategoryname TEXT NOT NULL,
    description     TEXT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_subcategories_category_name UNIQUE (fk_category, subcategoryname),
    CONSTRAINT fk_subcategories_category
        FOREIGN KEY (fk_category) REFERENCES categories (id_category)
);

CREATE INDEX IF NOT EXISTS ix_subcategories_fk_category ON subcategories (fk_category);

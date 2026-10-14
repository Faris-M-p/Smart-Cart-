-- =============================================================================
-- SmartCartPro | 005_Modules.sql
-- Scope: Platform-wide (permission grouping)
-- =============================================================================

CREATE TABLE IF NOT EXISTS modules (
    id_module       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    modulecode      TEXT NOT NULL,
    modulename      TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_modules_modulecode UNIQUE (modulecode)
);

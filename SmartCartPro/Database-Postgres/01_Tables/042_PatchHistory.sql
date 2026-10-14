-- =============================================================================
-- SmartCartPro | 042_PatchHistory.sql
-- Scope: Platform-wide (schema versioning)
-- =============================================================================

CREATE TABLE IF NOT EXISTS patch_history (
    id_patchhistory INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    version         TEXT NOT NULL,
    description     TEXT NULL,
    appliedat       TIMESTAMP NOT NULL DEFAULT NOW(),
    appliedby       TEXT NULL,
    success         BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_patch_history_version UNIQUE (version)
);

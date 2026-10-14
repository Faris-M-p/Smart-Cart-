-- =============================================================================
-- SmartCartPro | DatabaseVersion.sql
-- Ensures patch_history exists and records the current documented schema label.
-- Does not apply DDL patches; use BuildDatabase.bat / ApplyPatches.bat.
-- =============================================================================
\set ON_ERROR_STOP on

CREATE TABLE IF NOT EXISTS patch_history (
    id_patchhistory INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    version         TEXT NOT NULL,
    description     TEXT NULL,
    appliedat       TIMESTAMP NOT NULL DEFAULT NOW(),
    appliedby       TEXT NULL,
    success         BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_patch_history_version UNIQUE (version)
);

-- Logical schema version label for operators (not a Flyway replacement).
COMMENT ON TABLE patch_history IS 'SmartCartPro schema patch history; current baseline V001';

-- =============================================================================
-- SmartCartPro | 004_Roles.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS roles (
    id_role         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    rolecode        TEXT NOT NULL,
    rolename        TEXT NOT NULL,
    description     TEXT NULL,
    issystem        BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_roles_rolecode UNIQUE (rolecode)
);

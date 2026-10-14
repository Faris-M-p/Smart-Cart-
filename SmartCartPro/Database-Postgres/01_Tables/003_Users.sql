-- =============================================================================
-- SmartCartPro | 003_Users.sql
-- Scope: Company-wide (employees / admin users)
-- =============================================================================

CREATE TABLE IF NOT EXISTS users (
    id_user         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username        TEXT NOT NULL,
    email           TEXT NOT NULL,
    passwordhash    TEXT NOT NULL,
    fullname        TEXT NOT NULL,
    phone           TEXT NULL,
    employeecode    TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    mustchangepassword BOOLEAN NOT NULL DEFAULT TRUE,
    lastloginat     TIMESTAMP NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_users_isactive ON users (isactive) WHERE cancelled = FALSE;

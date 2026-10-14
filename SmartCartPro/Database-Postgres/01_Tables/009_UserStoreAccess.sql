-- =============================================================================
-- SmartCartPro | 009_UserStoreAccess.sql
-- Scope: Store-scoped (junction: which stores a user may access)
-- =============================================================================

CREATE TABLE IF NOT EXISTS user_store_access (
    id_userstoreaccess INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user         INT NOT NULL,
    fk_store        INT NOT NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_user_store_access UNIQUE (fk_user, fk_store),
    CONSTRAINT fk_user_store_access_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT fk_user_store_access_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_user_store_access_one_default
    ON user_store_access (fk_user)
    WHERE isdefault = TRUE AND cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_user_store_access_fk_store ON user_store_access (fk_store);

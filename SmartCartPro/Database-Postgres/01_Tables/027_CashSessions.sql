-- =============================================================================
-- SmartCartPro | 027_CashSessions.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS cash_sessions (
    id_cashsession  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_user         INT NOT NULL,
    openedat        TIMESTAMP NOT NULL DEFAULT NOW(),
    closedat        TIMESTAMP NULL,
    openingfloat    NUMERIC(12,2) NOT NULL DEFAULT 0,
    closingcash     NUMERIC(12,2) NULL,
    expectedcash    NUMERIC(12,2) NULL,
    variance        NUMERIC(12,2) NULL,
    status          TEXT NOT NULL DEFAULT 'open',
    notes           TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_cash_sessions_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_cash_sessions_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT ck_cash_sessions_status CHECK (status IN ('open','closed'))
);

CREATE INDEX IF NOT EXISTS ix_cash_sessions_fk_store ON cash_sessions (fk_store);
CREATE INDEX IF NOT EXISTS ix_cash_sessions_status ON cash_sessions (status);

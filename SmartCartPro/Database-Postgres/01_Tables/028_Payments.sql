-- =============================================================================
-- SmartCartPro | 028_Payments.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS payments (
    id_payment      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_sale         INT NULL,
    fk_cashsession  INT NULL,
    paymentmethod   TEXT NOT NULL,
    amount          NUMERIC(12,2) NOT NULL,
    referenceno     TEXT NULL,
    paidat          TIMESTAMP NOT NULL DEFAULT NOW(),
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_payments_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_payments_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_payments_cashsession
        FOREIGN KEY (fk_cashsession) REFERENCES cash_sessions (id_cashsession),
    CONSTRAINT fk_payments_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT ck_payments_amount CHECK (amount > 0),
    CONSTRAINT ck_payments_method
        CHECK (paymentmethod IN ('cash','card','upi','wallet','bank','other'))
);

CREATE INDEX IF NOT EXISTS ix_payments_fk_store ON payments (fk_store);
CREATE INDEX IF NOT EXISTS ix_payments_fk_sale ON payments (fk_sale);

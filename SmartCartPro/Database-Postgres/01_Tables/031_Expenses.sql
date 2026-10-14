-- =============================================================================
-- SmartCartPro | 031_Expenses.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS expenses (
    id_expense      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    expensedate     DATE NOT NULL DEFAULT CURRENT_DATE,
    category        TEXT NOT NULL,
    description     TEXT NULL,
    amount          NUMERIC(12,2) NOT NULL,
    paymentmethod   TEXT NULL,
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_expenses_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_expenses_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT ck_expenses_amount CHECK (amount >= 0)
);

CREATE INDEX IF NOT EXISTS ix_expenses_fk_store ON expenses (fk_store);
CREATE INDEX IF NOT EXISTS ix_expenses_expensedate ON expenses (expensedate);

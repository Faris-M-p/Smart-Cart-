-- =============================================================================
-- SmartCartPro | 041_InvoiceSequences.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS invoice_sequences (
    id_invoicesequence INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    sequencetype    TEXT NOT NULL,
    prefix          TEXT NOT NULL DEFAULT '',
    nextnumber      INT NOT NULL DEFAULT 1,
    padding         INT NOT NULL DEFAULT 6,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_invoice_sequences_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT uq_invoice_sequences UNIQUE (fk_store, sequencetype),
    CONSTRAINT ck_invoice_sequences_next CHECK (nextnumber >= 1)
);

CREATE INDEX IF NOT EXISTS ix_invoice_sequences_fk_store ON invoice_sequences (fk_store);

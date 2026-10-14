-- =============================================================================
-- SmartCartPro | 023_Purchases.sql
-- Scope: Store-scoped (receiving store)
-- =============================================================================

CREATE TABLE IF NOT EXISTS purchases (
    id_purchase     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_supplier     INT NOT NULL,
    purchasedate    DATE NOT NULL DEFAULT CURRENT_DATE,
    invoicenumber   TEXT NULL,
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    notes           TEXT NULL,
    status          TEXT NOT NULL DEFAULT 'posted',
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_purchases_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_purchases_supplier
        FOREIGN KEY (fk_supplier) REFERENCES suppliers (id_supplier),
    CONSTRAINT fk_purchases_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user)
);

CREATE INDEX IF NOT EXISTS ix_purchases_fk_store ON purchases (fk_store);
CREATE INDEX IF NOT EXISTS ix_purchases_fk_supplier ON purchases (fk_supplier);
CREATE INDEX IF NOT EXISTS ix_purchases_purchasedate ON purchases (purchasedate);

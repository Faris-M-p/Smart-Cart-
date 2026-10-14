-- =============================================================================
-- SmartCartPro | 025_Sales.sql
-- Scope: Store-scoped (POS / invoice)
-- =============================================================================

CREATE TABLE IF NOT EXISTS sales (
    id_sale         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NULL,
    fk_cashsession  INT NULL,
    invoicenumber   TEXT NOT NULL,
    saledate        TIMESTAMP NOT NULL DEFAULT NOW(),
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    paidamount      NUMERIC(12,2) NOT NULL DEFAULT 0,
    changegiven     NUMERIC(12,2) NOT NULL DEFAULT 0,
    status          TEXT NOT NULL DEFAULT 'completed',
    notes           TEXT NULL,
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_sales_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sales_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT fk_sales_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT uq_sales_store_invoicenumber UNIQUE (fk_store, invoicenumber)
);

CREATE INDEX IF NOT EXISTS ix_sales_fk_store ON sales (fk_store);
CREATE INDEX IF NOT EXISTS ix_sales_saledate ON sales (saledate);
CREATE INDEX IF NOT EXISTS ix_sales_fk_customer ON sales (fk_customer);

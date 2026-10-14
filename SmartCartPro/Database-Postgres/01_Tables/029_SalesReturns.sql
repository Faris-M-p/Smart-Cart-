-- =============================================================================
-- SmartCartPro | 029_SalesReturns.sql
-- Scope: Store-scoped
-- =============================================================================

CREATE TABLE IF NOT EXISTS sales_returns (
    id_salesreturn  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_sale         INT NOT NULL,
    returnnumber    TEXT NOT NULL,
    returndate      TIMESTAMP NOT NULL DEFAULT NOW(),
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    reason          TEXT NULL,
    status          TEXT NOT NULL DEFAULT 'completed',
    createdby       INT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_sales_returns_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_sales_returns_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_sales_returns_createdby
        FOREIGN KEY (createdby) REFERENCES users (id_user),
    CONSTRAINT uq_sales_returns_store_number UNIQUE (fk_store, returnnumber)
);

CREATE INDEX IF NOT EXISTS ix_sales_returns_fk_store ON sales_returns (fk_store);
CREATE INDEX IF NOT EXISTS ix_sales_returns_fk_sale ON sales_returns (fk_sale);

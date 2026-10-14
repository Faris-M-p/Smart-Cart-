-- =============================================================================
-- SmartCartPro | 034_OnlineOrders.sql
-- Scope: Store-scoped (fulfillment store)
-- =============================================================================

CREATE TABLE IF NOT EXISTS online_orders (
    id_onlineorder  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_store        INT NOT NULL,
    fk_customer     INT NOT NULL,
    ordernumber     TEXT NOT NULL,
    orderdate       TIMESTAMP NOT NULL DEFAULT NOW(),
    status          TEXT NOT NULL DEFAULT 'pending',
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    taxamount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    shippingamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    discountamount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    shippingaddress TEXT NULL,
    notes           TEXT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_online_orders_store
        FOREIGN KEY (fk_store) REFERENCES store (id_store),
    CONSTRAINT fk_online_orders_customer
        FOREIGN KEY (fk_customer) REFERENCES customers (id_customer),
    CONSTRAINT uq_online_orders_store_number UNIQUE (fk_store, ordernumber)
);

CREATE INDEX IF NOT EXISTS ix_online_orders_fk_store ON online_orders (fk_store);
CREATE INDEX IF NOT EXISTS ix_online_orders_fk_customer ON online_orders (fk_customer);
CREATE INDEX IF NOT EXISTS ix_online_orders_status ON online_orders (status);

CREATE TABLE IF NOT EXISTS purchase (
    id_purchase      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_supplier      INT NOT NULL,
    purchase_date    DATE NOT NULL DEFAULT CURRENT_DATE,
    invoice_number   TEXT NULL,
    total_amount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    notes            TEXT NULL,
    created_on       TIMESTAMP NOT NULL DEFAULT NOW(),
    enter_by         INT NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    cancelled_by     INT NULL,
    CONSTRAINT fk_purchase_supplier
        FOREIGN KEY (fk_supplier) REFERENCES supplier (id_supplier)
);

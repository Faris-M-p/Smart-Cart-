CREATE TABLE IF NOT EXISTS purchase (
    id_purchase     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_supplier     INT NOT NULL,
    purchasedate    DATE NOT NULL DEFAULT CURRENT_DATE,
    invoicenumber   TEXT NULL,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    notes           TEXT NULL,
    createdon       TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby         INT NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    cancelledby     INT NULL,
    CONSTRAINT fk_purchase_supplier
        FOREIGN KEY (fk_supplier) REFERENCES supplier (id_supplier)
);

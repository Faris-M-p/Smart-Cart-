CREATE TABLE IF NOT EXISTS sales (
    id_sale         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    invoicenumber   TEXT NULL,
    saledate        DATE NOT NULL DEFAULT CURRENT_DATE,
    customername    TEXT NULL,
    customerphone   TEXT NULL,
    paymentmethod   TEXT NOT NULL DEFAULT 'Cash',
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    notes           TEXT NULL,
    createdon       TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby         INT NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    cancelledby     INT NULL
);

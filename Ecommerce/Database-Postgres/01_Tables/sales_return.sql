CREATE TABLE IF NOT EXISTS salesreturn (
    id_salesreturn  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_sale         INT NOT NULL,
    invoicenumber   TEXT NULL,
    returndate      DATE NOT NULL DEFAULT CURRENT_DATE,
    totalamount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    reason          TEXT NULL,
    notes           TEXT NULL,
    createdon       TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby         INT NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    cancelledby     INT NULL,
    CONSTRAINT fk_salesreturn_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale)
);

CREATE TABLE IF NOT EXISTS payments (
    id_payment      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_order        INT NOT NULL,
    paymentdate     TIMESTAMP NULL DEFAULT NOW(),
    paymentamount   NUMERIC(10,2) NOT NULL,
    paymentstatus   TEXT NOT NULL,
    paymentmethod   TEXT NOT NULL,
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_payments_order
        FOREIGN KEY (fk_order) REFERENCES orders (id_order)
);

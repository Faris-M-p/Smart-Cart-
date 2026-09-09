CREATE TABLE IF NOT EXISTS payments (
    payment_id       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id         INT NOT NULL,
    payment_date     TIMESTAMP NULL DEFAULT NOW(),
    payment_amount   NUMERIC(10,2) NOT NULL,
    payment_status   TEXT NOT NULL,
    payment_method   TEXT NOT NULL,
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT fk_payments_order
        FOREIGN KEY (order_id) REFERENCES orders (order_id)
);

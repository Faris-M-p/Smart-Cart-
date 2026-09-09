/* orders — folded checkout fields from Orders_AddCheckoutFields */
CREATE TABLE IF NOT EXISTS orders (
    order_id         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id          INT NOT NULL,
    order_date       TIMESTAMP NULL DEFAULT NOW(),
    total_amount     NUMERIC(10,2) NOT NULL,
    order_status     TEXT NOT NULL,
    shipping_address TEXT NULL,
    payment_method   TEXT NOT NULL,
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    order_number     TEXT NULL,
    receiver_name    TEXT NULL,
    phone            TEXT NULL,
    address_line     TEXT NULL,
    city             TEXT NULL,
    pincode          TEXT NULL,
    CONSTRAINT fk_orders_user
        FOREIGN KEY (user_id) REFERENCES users (user_id)
);

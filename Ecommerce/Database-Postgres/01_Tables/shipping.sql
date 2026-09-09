CREATE TABLE IF NOT EXISTS shipping (
    shipping_id             INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id                INT NOT NULL,
    shipping_address        TEXT NOT NULL,
    shipping_date           TIMESTAMP NULL,
    estimated_delivery_date TIMESTAMP NULL,
    shipping_status         TEXT NOT NULL,
    cancelled               BOOLEAN NULL DEFAULT FALSE,
    cancelled_on            TIMESTAMP NULL,
    cancelled_reason        TEXT NULL,
    CONSTRAINT fk_shipping_order
        FOREIGN KEY (order_id) REFERENCES orders (order_id)
);

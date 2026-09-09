CREATE TABLE IF NOT EXISTS shipping (
    id_shipping            INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_order               INT NOT NULL,
    shippingaddress        TEXT NOT NULL,
    shippingdate           TIMESTAMP NULL,
    estimateddeliverydate  TIMESTAMP NULL,
    shippingstatus         TEXT NOT NULL,
    cancelled              BOOLEAN NULL DEFAULT FALSE,
    cancelledon            TIMESTAMP NULL,
    cancelledreason        TEXT NULL,
    CONSTRAINT fk_shipping_order
        FOREIGN KEY (fk_order) REFERENCES orders (id_order)
);

CREATE TABLE IF NOT EXISTS orders (
    id_order        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user         INT NOT NULL,
    orderdate       TIMESTAMP NULL DEFAULT NOW(),
    totalamount     NUMERIC(10,2) NOT NULL,
    orderstatus     TEXT NOT NULL,
    shippingaddress TEXT NULL,
    paymentmethod   TEXT NOT NULL,
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    ordernumber     TEXT NULL,
    receivername    TEXT NULL,
    phone           TEXT NULL,
    addressline     TEXT NULL,
    city            TEXT NULL,
    pincode         TEXT NULL,
    CONSTRAINT fk_orders_users
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

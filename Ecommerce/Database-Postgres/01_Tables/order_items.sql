CREATE TABLE IF NOT EXISTS orderitems (
    id_orderitem      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_order          INT NOT NULL,
    fk_product        INT NOT NULL,
    fk_productvariant INT NULL,
    productname       TEXT NOT NULL,
    variantlabel      TEXT NULL,
    sku               TEXT NULL,
    unitprice         NUMERIC(10,2) NOT NULL,
    quantity          INT NOT NULL,
    linetotal         NUMERIC(10,2) NOT NULL,
    createdat         TIMESTAMP NULL DEFAULT NOW(),
    CONSTRAINT fk_orderitems_order
        FOREIGN KEY (fk_order) REFERENCES orders (id_order),
    CONSTRAINT fk_orderitems_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_orderitems_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

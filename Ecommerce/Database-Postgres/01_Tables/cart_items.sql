CREATE TABLE IF NOT EXISTS cartitems (
    id_cartitem       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_cart           INT NOT NULL,
    fk_product        INT NOT NULL,
    fk_productvariant INT NULL,
    quantity         INT NOT NULL,
    price            NUMERIC(10,2) NOT NULL,
    createdat        TIMESTAMP NULL DEFAULT NOW(),
    CONSTRAINT fk_cartitems_cart
        FOREIGN KEY (fk_cart) REFERENCES cart (id_cart),
    CONSTRAINT fk_cartitems_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_cartitems_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

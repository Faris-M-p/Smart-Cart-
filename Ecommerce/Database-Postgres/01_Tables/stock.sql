CREATE TABLE IF NOT EXISTS stock (
    id_stock          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchasedetail INT NOT NULL,
    fk_productvariant INT NOT NULL,
    quantity          INT NOT NULL,
    createdon         TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby           INT NULL,
    cancelled         BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon       TIMESTAMP NULL,
    cancelledreason   TEXT NULL,
    cancelledby       INT NULL,
    CONSTRAINT fk_stock_purchasedetail
        FOREIGN KEY (fk_purchasedetail) REFERENCES purchasedetail (id_purchasedetail),
    CONSTRAINT fk_stock_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

CREATE TABLE IF NOT EXISTS purchasedetail (
    id_purchasedetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_purchase       INT NOT NULL,
    fk_productvariant INT NOT NULL,
    quantity          INT NOT NULL,
    purchaseprice     NUMERIC(18,2) NOT NULL DEFAULT 0,
    mrp               NUMERIC(18,2) NULL,
    expirydate        DATE NULL,
    createdon         TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby           INT NULL,
    cancelled         BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon       TIMESTAMP NULL,
    cancelledreason   TEXT NULL,
    cancelledby       INT NULL,
    CONSTRAINT fk_purchasedetail_purchase
        FOREIGN KEY (fk_purchase) REFERENCES purchase (id_purchase),
    CONSTRAINT fk_purchasedetail_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

CREATE TABLE IF NOT EXISTS salesdetail (
    id_salesdetail    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_sale           INT NOT NULL,
    fk_productvariant INT NOT NULL,
    quantity          INT NOT NULL,
    sellingprice      NUMERIC(18,2) NOT NULL DEFAULT 0,
    mrp               NUMERIC(18,2) NULL,
    createdon         TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby           INT NULL,
    cancelled         BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon       TIMESTAMP NULL,
    cancelledreason   TEXT NULL,
    cancelledby       INT NULL,
    CONSTRAINT fk_salesdetail_sale
        FOREIGN KEY (fk_sale) REFERENCES sales (id_sale),
    CONSTRAINT fk_salesdetail_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

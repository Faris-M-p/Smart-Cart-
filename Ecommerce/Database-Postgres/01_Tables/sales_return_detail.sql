CREATE TABLE IF NOT EXISTS salesreturndetail (
    id_salesreturndetail INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_salesreturn       INT NOT NULL,
    fk_salesdetail       INT NOT NULL,
    fk_productvariant    INT NOT NULL,
    quantity             INT NOT NULL,
    sellingprice         NUMERIC(18,2) NOT NULL DEFAULT 0,
    createdon            TIMESTAMP NOT NULL DEFAULT NOW(),
    enterby              INT NULL,
    cancelled            BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon          TIMESTAMP NULL,
    cancelledreason      TEXT NULL,
    cancelledby          INT NULL,
    CONSTRAINT fk_salesreturndetail_return
        FOREIGN KEY (fk_salesreturn) REFERENCES salesreturn (id_salesreturn),
    CONSTRAINT fk_salesreturndetail_salesdetail
        FOREIGN KEY (fk_salesdetail) REFERENCES salesdetail (id_salesdetail),
    CONSTRAINT fk_salesreturndetail_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

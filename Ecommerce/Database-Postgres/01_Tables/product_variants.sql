CREATE TABLE IF NOT EXISTS productvariants (
    id_productvariant INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product        INT NOT NULL,
    sku               TEXT NOT NULL,
    barcode           TEXT NULL,
    variantlabel      TEXT NOT NULL,
    description       TEXT NULL,
    mrp               NUMERIC(10,2) NOT NULL,
    sellingprice      NUMERIC(10,2) NOT NULL,
    unitofmeasure     TEXT NULL,
    unitvalue         NUMERIC(10,3) NULL,
    isdefault         BOOLEAN DEFAULT FALSE,
    maxorderqty       INT DEFAULT 10,
    isactive          BOOLEAN DEFAULT TRUE,
    sellonline        BOOLEAN NOT NULL DEFAULT FALSE,
    createdat         TIMESTAMP DEFAULT NOW(),
    cancelled         BOOLEAN DEFAULT FALSE,
    cancelledon       TIMESTAMP NULL,
    CONSTRAINT uq_productvariants_sku UNIQUE (sku),
    CONSTRAINT uq_productvariants_barcode UNIQUE (barcode),
    CONSTRAINT fk_productvariants_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product)
);

CREATE TABLE IF NOT EXISTS productimages (
    id_productimage INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    imageurl        TEXT NOT NULL,
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_productimages_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product)
);

CREATE TABLE IF NOT EXISTS wishlistitems (
    id_wishlistitem INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_wishlist     INT NOT NULL,
    fk_product      INT NOT NULL,
    createdat       TIMESTAMP NULL DEFAULT NOW(),
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_wishlistitems_wishlist
        FOREIGN KEY (fk_wishlist) REFERENCES wishlist (id_wishlist),
    CONSTRAINT fk_wishlistitems_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product)
);

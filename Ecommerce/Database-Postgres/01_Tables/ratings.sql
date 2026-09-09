CREATE TABLE IF NOT EXISTS ratings (
    id_rating       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    fk_user         INT NOT NULL,
    ratingvalue     NUMERIC(2,1) NOT NULL,
    review          TEXT NULL,
    createdat       TIMESTAMP NULL DEFAULT NOW(),
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_ratings_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT fk_ratings_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

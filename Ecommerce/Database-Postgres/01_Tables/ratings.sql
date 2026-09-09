CREATE TABLE IF NOT EXISTS ratings (
    rating_id        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id       INT NOT NULL,
    user_id          INT NOT NULL,
    rating_value     NUMERIC(2,1) NOT NULL,
    review           TEXT NULL,
    created_at       TIMESTAMP NULL DEFAULT NOW(),
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT fk_ratings_product
        FOREIGN KEY (product_id) REFERENCES products (id_product),
    CONSTRAINT fk_ratings_user
        FOREIGN KEY (user_id) REFERENCES users (user_id)
);

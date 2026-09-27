CREATE TABLE IF NOT EXISTS homepage_products (
    id_homepageproduct INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product         INT NOT NULL UNIQUE REFERENCES products (id_product),
    isactive           BOOLEAN NOT NULL DEFAULT TRUE,
    displayorder       INT NOT NULL DEFAULT 0,
    createdat          TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat          TIMESTAMP NULL,
    cancelled          BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon        TIMESTAMP NULL,
    cancelledreason    TEXT NULL
);

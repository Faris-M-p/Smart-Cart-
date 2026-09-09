CREATE TABLE IF NOT EXISTS products (
    id_product     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_subcategory INT NOT NULL,
    fk_brand       INT NULL,
    name           TEXT NOT NULL,
    slug           TEXT NOT NULL,
    description    TEXT NULL,
    isactive       BOOLEAN NOT NULL DEFAULT TRUE,
    sellonline     BOOLEAN NOT NULL DEFAULT FALSE,
    createdat      TIMESTAMP NULL DEFAULT NOW(),
    modifiedat     TIMESTAMP NULL,
    cancelled      BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon    TIMESTAMP NULL,
    CONSTRAINT uq_products_slug UNIQUE (slug),
    CONSTRAINT fk_products_subcategory
        FOREIGN KEY (fk_subcategory) REFERENCES subcategory (id_subcategory),
    CONSTRAINT fk_products_brand
        FOREIGN KEY (fk_brand) REFERENCES brand (id_brand)
);

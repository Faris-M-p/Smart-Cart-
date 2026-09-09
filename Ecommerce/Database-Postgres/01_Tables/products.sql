CREATE TABLE IF NOT EXISTS products (
    id_product     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_subcategory INT NOT NULL,
    fk_brand       INT NULL,
    name           TEXT NOT NULL,
    slug           TEXT NOT NULL,
    description    TEXT NULL,
    is_active      BOOLEAN NOT NULL DEFAULT TRUE,
    sell_online    BOOLEAN NOT NULL DEFAULT FALSE,
    created_at     TIMESTAMP NULL DEFAULT NOW(),
    modified_at    TIMESTAMP NULL,
    cancelled      BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on   TIMESTAMP NULL,
    CONSTRAINT uq_products_slug UNIQUE (slug),
    CONSTRAINT fk_products_subcategory
        FOREIGN KEY (fk_subcategory) REFERENCES subcategory (id_subcategory),
    CONSTRAINT fk_products_brand
        FOREIGN KEY (fk_brand) REFERENCES brand (id_brand)
);

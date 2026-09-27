CREATE TABLE IF NOT EXISTS homepage_categories (
    id_homepagecategory INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_category         INT NOT NULL UNIQUE REFERENCES category (id_category),
    isactive            BOOLEAN NOT NULL DEFAULT TRUE,
    displayorder        INT NOT NULL DEFAULT 0,
    createdat           TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat           TIMESTAMP NULL,
    cancelled           BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon         TIMESTAMP NULL,
    cancelledreason     TEXT NULL
);

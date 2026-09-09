CREATE TABLE IF NOT EXISTS subcategory (
    id_subcategory   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name             TEXT NOT NULL,
    description      TEXT NULL,
    fk_category      INT NOT NULL,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    image_url        TEXT NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT fk_subcategory_category
        FOREIGN KEY (fk_category) REFERENCES category (id_category)
);

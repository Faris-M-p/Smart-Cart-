CREATE TABLE IF NOT EXISTS subcategory (
    id_subcategory  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            TEXT NOT NULL,
    description     TEXT NULL,
    fk_category     INT NOT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    imageurl        TEXT NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_subcategory_category
        FOREIGN KEY (fk_category) REFERENCES category (id_category)
);

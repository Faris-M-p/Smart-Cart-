CREATE TABLE IF NOT EXISTS brand (
    id_brand        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    brandname       TEXT NOT NULL,
    description     TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    imageurl        TEXT NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL
);

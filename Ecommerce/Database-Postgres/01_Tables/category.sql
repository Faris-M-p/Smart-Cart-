CREATE TABLE IF NOT EXISTS category (
    id_category     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            TEXT NOT NULL,
    description     TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    imageurl        TEXT NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL
);

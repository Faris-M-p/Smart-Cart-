CREATE TABLE IF NOT EXISTS category (
    id_category      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name             TEXT NOT NULL,
    description      TEXT NULL,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    image_url        TEXT NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL
);

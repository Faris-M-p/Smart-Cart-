CREATE TABLE IF NOT EXISTS brand (
    id_brand         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    brand_name       TEXT NOT NULL,
    description      TEXT NULL,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    image_url        TEXT NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL
);

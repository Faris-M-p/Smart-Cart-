CREATE TABLE IF NOT EXISTS product_status (
    status_id        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    status_name      TEXT NOT NULL,
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL
);

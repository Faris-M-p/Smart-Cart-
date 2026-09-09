CREATE TABLE IF NOT EXISTS supplier (
    id_supplier      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name             TEXT NOT NULL,
    company_name     TEXT NULL,
    email            TEXT NULL,
    phone            TEXT NULL,
    state            TEXT NOT NULL,
    district         TEXT NOT NULL,
    city             TEXT NOT NULL,
    address          TEXT NULL,
    pincode          TEXT NULL,
    description      TEXT NULL,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT uq_supplier_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_supplier_name ON supplier (name);

CREATE TABLE IF NOT EXISTS supplier (
    id_supplier     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            TEXT NOT NULL,
    companyname     TEXT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    state           TEXT NOT NULL,
    district        TEXT NOT NULL,
    city            TEXT NOT NULL,
    address         TEXT NULL,
    pincode         TEXT NULL,
    description     TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_supplier_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_supplier_name ON supplier (name);

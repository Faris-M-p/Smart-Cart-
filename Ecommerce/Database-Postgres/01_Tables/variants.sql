CREATE TABLE IF NOT EXISTS variants (
    id_variant   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name         TEXT NOT NULL,
    description  TEXT NULL,
    displayorder INT DEFAULT 0,
    isactive     BOOLEAN DEFAULT TRUE,
    cancelled    BOOLEAN DEFAULT FALSE,
    cancelledon  TIMESTAMP NULL,
    CONSTRAINT uq_variants_name UNIQUE (name)
);

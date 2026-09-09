CREATE TABLE IF NOT EXISTS variants (
    id_variant    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name          TEXT NOT NULL,
    description   TEXT NULL,
    display_order INT DEFAULT 0,
    is_active     BOOLEAN DEFAULT TRUE,
    cancelled     BOOLEAN DEFAULT FALSE,
    cancelled_on  TIMESTAMP NULL,
    CONSTRAINT uq_variants_name UNIQUE (name)
);

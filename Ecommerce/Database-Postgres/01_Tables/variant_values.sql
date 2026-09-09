CREATE TABLE IF NOT EXISTS variantvalues (
    id_variantvalue INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_variant      INT NOT NULL,
    name            TEXT NOT NULL,
    description     TEXT NULL,
    displayorder    INT DEFAULT 0,
    cancelled       BOOLEAN DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    CONSTRAINT fk_variantvalues_variant
        FOREIGN KEY (fk_variant) REFERENCES variants (id_variant),
    CONSTRAINT uq_variant_value UNIQUE (fk_variant, name)
);

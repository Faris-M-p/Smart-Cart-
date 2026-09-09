CREATE TABLE IF NOT EXISTS variant_values (
    id_variant_value INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_variant       INT NOT NULL,
    name             TEXT NOT NULL,
    description      TEXT NULL,
    display_order    INT DEFAULT 0,
    cancelled        BOOLEAN DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    CONSTRAINT fk_variant_values_variant
        FOREIGN KEY (fk_variant) REFERENCES variants (id_variant),
    CONSTRAINT uq_variant_value UNIQUE (fk_variant, name)
);

CREATE TABLE IF NOT EXISTS product_variant_attributes (
    id_product_variant_attribute INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product_variant           INT NOT NULL,
    fk_variant                   INT NOT NULL,
    fk_variant_value             INT NOT NULL,
    description                  TEXT NULL,
    CONSTRAINT fk_pva_product_variant
        FOREIGN KEY (fk_product_variant) REFERENCES product_variants (id_product_variant),
    CONSTRAINT fk_pva_variant
        FOREIGN KEY (fk_variant) REFERENCES variants (id_variant),
    CONSTRAINT fk_pva_variant_value
        FOREIGN KEY (fk_variant_value) REFERENCES variant_values (id_variant_value),
    CONSTRAINT uq_product_variant_attributes UNIQUE (fk_product_variant, fk_variant)
);

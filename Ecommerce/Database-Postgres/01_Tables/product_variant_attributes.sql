CREATE TABLE IF NOT EXISTS productvariantattributes (
    id_productvariantattribute INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_productvariant          INT NOT NULL,
    fk_variant                 INT NOT NULL,
    fk_variantvalue            INT NOT NULL,
    description                TEXT NULL,
    CONSTRAINT fk_pva_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant),
    CONSTRAINT fk_pva_variant
        FOREIGN KEY (fk_variant) REFERENCES variants (id_variant),
    CONSTRAINT fk_pva_variantvalue
        FOREIGN KEY (fk_variantvalue) REFERENCES variantvalues (id_variantvalue),
    CONSTRAINT uq_productvariantattributes UNIQUE (fk_productvariant, fk_variant)
);

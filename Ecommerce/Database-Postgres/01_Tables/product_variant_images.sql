CREATE TABLE IF NOT EXISTS productvariantimages (
    id_productvariantimage INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_productvariant      INT NOT NULL,
    imageurl               TEXT NOT NULL,
    isprimary              BOOLEAN NOT NULL DEFAULT FALSE,
    displayorder           INT NOT NULL DEFAULT 0,
    createdat              TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_productvariantimages_productvariant
        FOREIGN KEY (fk_productvariant) REFERENCES productvariants (id_productvariant)
);

CREATE INDEX IF NOT EXISTS ix_productvariantimages_fk_productvariant
    ON productvariantimages (fk_productvariant);

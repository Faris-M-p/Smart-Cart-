CREATE TABLE IF NOT EXISTS productmedia (
    id_productmedia INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_product      INT NOT NULL,
    mediatype       TEXT NOT NULL,
    mediaurl        TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isprimary       BOOLEAN NOT NULL DEFAULT FALSE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT fk_productmedia_product
        FOREIGN KEY (fk_product) REFERENCES products (id_product),
    CONSTRAINT ck_productmedia_mediatype
        CHECK (mediatype IN ('Image', 'Video'))
);

CREATE INDEX IF NOT EXISTS ix_productmedia_fk_product
    ON productmedia (fk_product);

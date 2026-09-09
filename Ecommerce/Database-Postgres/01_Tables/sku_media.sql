CREATE TABLE IF NOT EXISTS skumedia (
    id_skumedia   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_productsku INT NOT NULL,
    mediatype     TEXT NOT NULL DEFAULT 'Image',
    mediaurl      TEXT NOT NULL,
    displayorder  INT NOT NULL DEFAULT 0,
    isprimary     BOOLEAN NOT NULL DEFAULT FALSE,
    createdat     TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat     TIMESTAMP NULL,
    CONSTRAINT fk_skumedia_productvariant
        FOREIGN KEY (fk_productsku) REFERENCES productvariants (id_productvariant),
    CONSTRAINT ck_skumedia_mediatype
        CHECK (mediatype = 'Image')
);

CREATE INDEX IF NOT EXISTS ix_skumedia_fk_productsku
    ON skumedia (fk_productsku, displayorder);

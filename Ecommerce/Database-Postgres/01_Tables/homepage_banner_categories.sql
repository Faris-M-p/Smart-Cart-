CREATE TABLE IF NOT EXISTS homepage_banner_categories (
    id_bannercategory INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_banner         INT NOT NULL REFERENCES homepage_banners (id_banner) ON DELETE CASCADE,
    fk_category       INT NOT NULL REFERENCES category (id_category)
);

CREATE INDEX IF NOT EXISTS ix_hbc_banner ON homepage_banner_categories (fk_banner);
CREATE INDEX IF NOT EXISTS ix_hbc_category ON homepage_banner_categories (fk_category);

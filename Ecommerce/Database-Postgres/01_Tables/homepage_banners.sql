CREATE TABLE IF NOT EXISTS homepage_banners (
    id_banner       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title           TEXT NULL,
    imageurl        TEXT NOT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    displayorder    INT NOT NULL DEFAULT 0,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL
);

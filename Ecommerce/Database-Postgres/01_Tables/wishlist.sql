CREATE TABLE IF NOT EXISTS wishlist (
    id_wishlist     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user         INT NOT NULL,
    sessionkey      UUID NULL,
    createdat       TIMESTAMP NULL DEFAULT NOW(),
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_wishlist_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_wishlist_sessionkey
    ON wishlist (sessionkey)
    WHERE sessionkey IS NOT NULL;

CREATE TABLE IF NOT EXISTS cart (
    id_cart    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user    INT NOT NULL,
    sessionkey UUID NULL,
    createdat  TIMESTAMP NULL DEFAULT NOW(),
    CONSTRAINT fk_cart_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_cart_sessionkey
    ON cart (sessionkey)
    WHERE sessionkey IS NOT NULL;

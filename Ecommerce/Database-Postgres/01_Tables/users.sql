CREATE TABLE IF NOT EXISTS users (
    id_user         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username        TEXT NOT NULL,
    fullname        TEXT NULL,
    passwordhash    TEXT NOT NULL,
    email           TEXT NOT NULL,
    phonenumber     TEXT NULL,
    isadmin         BOOLEAN NOT NULL,
    createdat       TIMESTAMP NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_email ON users (email);

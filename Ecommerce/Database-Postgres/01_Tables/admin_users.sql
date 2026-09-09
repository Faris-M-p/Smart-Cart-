CREATE TABLE IF NOT EXISTS adminusers (
    id_adminuser    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_userrole     INT NOT NULL,
    username        TEXT NOT NULL,
    passwordhash    TEXT NOT NULL,
    fullname        TEXT NOT NULL,
    email           TEXT NOT NULL,
    phonenumber     TEXT NULL,
    profileimageurl TEXT NULL,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_adminusers_username UNIQUE (username),
    CONSTRAINT fk_adminusers_userrole
        FOREIGN KEY (fk_userrole) REFERENCES userroles (id_userrole)
);

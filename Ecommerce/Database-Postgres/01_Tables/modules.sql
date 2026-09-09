CREATE TABLE IF NOT EXISTS modules (
    id_module       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    modulename      TEXT NOT NULL,
    displayname     TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_modules_modulename UNIQUE (modulename)
);

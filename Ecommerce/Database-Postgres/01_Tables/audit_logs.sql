CREATE TABLE IF NOT EXISTS auditlogs (
    id_auditlog     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    action          TEXT NOT NULL,
    fk_user         INT NOT NULL,
    tablename       TEXT NOT NULL,
    recordid        INT NOT NULL,
    changedetails   TEXT NULL,
    createdat       TIMESTAMP NULL DEFAULT NOW(),
    cancelled       BOOLEAN NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_auditlogs_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

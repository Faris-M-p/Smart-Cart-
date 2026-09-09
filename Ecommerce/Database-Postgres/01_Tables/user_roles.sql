CREATE TABLE IF NOT EXISTS userroles (
    id_userrole     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    rolename        TEXT NOT NULL,
    description     TEXT NULL,
    issystemrole    BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_userroles_rolename UNIQUE (rolename)
);

CREATE TABLE IF NOT EXISTS permissions (
    id_permission   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_module       INT NOT NULL,
    permissionname  TEXT NOT NULL,
    permissioncode  TEXT NOT NULL,
    displayorder    INT NOT NULL DEFAULT 0,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT uq_permissions_permissioncode UNIQUE (permissioncode),
    CONSTRAINT fk_permissions_module
        FOREIGN KEY (fk_module) REFERENCES modules (id_module)
);

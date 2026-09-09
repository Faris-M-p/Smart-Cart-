CREATE TABLE IF NOT EXISTS userrolepermissions (
    id_userrolepermission INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_userrole           INT NOT NULL,
    fk_permission         INT NOT NULL,
    createdat             TIMESTAMP NOT NULL DEFAULT NOW(),
    cancelled             BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon           TIMESTAMP NULL,
    cancelledreason       TEXT NULL,
    CONSTRAINT uq_userrolepermissions_role_permission UNIQUE (fk_userrole, fk_permission),
    CONSTRAINT fk_userrolepermissions_userrole
        FOREIGN KEY (fk_userrole) REFERENCES userroles (id_userrole),
    CONSTRAINT fk_userrolepermissions_permission
        FOREIGN KEY (fk_permission) REFERENCES permissions (id_permission)
);

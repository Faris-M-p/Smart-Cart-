CREATE TABLE IF NOT EXISTS user_role_permissions (
    id_user_role_permission INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user_role            INT NOT NULL,
    fk_permission           INT NOT NULL,
    created_at              TIMESTAMP NOT NULL DEFAULT NOW(),
    cancelled               BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on            TIMESTAMP NULL,
    cancelled_reason        TEXT NULL,
    CONSTRAINT uq_user_role_permissions_role_permission
        UNIQUE (fk_user_role, fk_permission),
    CONSTRAINT fk_user_role_permissions_user_role
        FOREIGN KEY (fk_user_role) REFERENCES user_roles (id_user_role),
    CONSTRAINT fk_user_role_permissions_permission
        FOREIGN KEY (fk_permission) REFERENCES permissions (id_permission)
);

-- =============================================================================
-- SmartCartPro | 007_RolePermissions.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS role_permissions (
    id_rolepermission INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_role         INT NOT NULL,
    fk_permission   INT NOT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT uq_role_permissions UNIQUE (fk_role, fk_permission),
    CONSTRAINT fk_role_permissions_role
        FOREIGN KEY (fk_role) REFERENCES roles (id_role),
    CONSTRAINT fk_role_permissions_permission
        FOREIGN KEY (fk_permission) REFERENCES permissions (id_permission)
);

CREATE INDEX IF NOT EXISTS ix_role_permissions_fk_role ON role_permissions (fk_role);
CREATE INDEX IF NOT EXISTS ix_role_permissions_fk_permission ON role_permissions (fk_permission);

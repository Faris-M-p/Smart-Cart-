-- =============================================================================
-- SmartCartPro | 008_UserRoles.sql
-- Scope: Company-wide
-- =============================================================================

CREATE TABLE IF NOT EXISTS user_roles (
    id_userrole     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user         INT NOT NULL,
    fk_role         INT NOT NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    CONSTRAINT uq_user_roles UNIQUE (fk_user, fk_role),
    CONSTRAINT fk_user_roles_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user),
    CONSTRAINT fk_user_roles_role
        FOREIGN KEY (fk_role) REFERENCES roles (id_role)
);

CREATE INDEX IF NOT EXISTS ix_user_roles_fk_user ON user_roles (fk_user);
CREATE INDEX IF NOT EXISTS ix_user_roles_fk_role ON user_roles (fk_role);

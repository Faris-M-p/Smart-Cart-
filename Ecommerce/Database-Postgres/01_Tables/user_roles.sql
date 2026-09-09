CREATE TABLE IF NOT EXISTS user_roles (
    id_user_role     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    role_name        TEXT NOT NULL,
    description      TEXT NULL,
    is_system_role   BOOLEAN NOT NULL DEFAULT FALSE,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT uq_user_roles_role_name UNIQUE (role_name)
);

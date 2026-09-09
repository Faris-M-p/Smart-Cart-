CREATE TABLE IF NOT EXISTS permissions (
    id_permission    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_module        INT NOT NULL,
    permission_name  TEXT NOT NULL,
    permission_code  TEXT NOT NULL,
    display_order    INT NOT NULL DEFAULT 0,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT uq_permissions_permission_code UNIQUE (permission_code),
    CONSTRAINT fk_permissions_module
        FOREIGN KEY (fk_module) REFERENCES modules (id_module)
);

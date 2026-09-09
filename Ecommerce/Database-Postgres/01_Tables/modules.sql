CREATE TABLE IF NOT EXISTS modules (
    id_module        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    module_name      TEXT NOT NULL,
    display_name     TEXT NOT NULL,
    display_order    INT NOT NULL DEFAULT 0,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT uq_modules_module_name UNIQUE (module_name)
);

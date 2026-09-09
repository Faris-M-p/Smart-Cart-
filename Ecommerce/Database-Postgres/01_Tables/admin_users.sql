CREATE TABLE IF NOT EXISTS admin_users (
    id_admin_user    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_user_role     INT NOT NULL,
    user_name        TEXT NOT NULL,
    password_hash    TEXT NOT NULL,
    full_name        TEXT NOT NULL,
    email            TEXT NOT NULL,
    phone_number     TEXT NULL,
    profile_image_url TEXT NULL,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    cancelled        BOOLEAN NOT NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL,
    CONSTRAINT uq_admin_users_user_name UNIQUE (user_name),
    CONSTRAINT fk_admin_users_user_role
        FOREIGN KEY (fk_user_role) REFERENCES user_roles (id_user_role)
);

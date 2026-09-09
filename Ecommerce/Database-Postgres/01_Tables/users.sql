/* users — folded FullName + UX_Users_Email */
CREATE TABLE IF NOT EXISTS users (
    user_id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_name        TEXT NOT NULL,
    full_name        TEXT NULL,
    password_hash    TEXT NOT NULL,
    email            TEXT NOT NULL,
    phone_number     TEXT NULL,
    is_admin         BOOLEAN NOT NULL,
    created_at       TIMESTAMP NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NULL,
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_email ON users (email);

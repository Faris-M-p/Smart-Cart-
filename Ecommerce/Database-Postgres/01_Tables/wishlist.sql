CREATE TABLE IF NOT EXISTS wishlist (
    wishlist_id      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id          INT NOT NULL,
    session_key      UUID NULL,
    created_at       TIMESTAMP NULL DEFAULT NOW(),
    cancelled        BOOLEAN NULL DEFAULT FALSE,
    cancelled_on     TIMESTAMP NULL,
    cancelled_reason TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_wishlist_session_key
    ON wishlist (session_key)
    WHERE session_key IS NOT NULL;

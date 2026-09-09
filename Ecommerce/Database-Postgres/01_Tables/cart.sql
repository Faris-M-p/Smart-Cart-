CREATE TABLE IF NOT EXISTS cart (
    cart_id     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id     INT NOT NULL,
    session_key UUID NULL,
    created_at  TIMESTAMP NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_cart_session_key
    ON cart (session_key)
    WHERE session_key IS NOT NULL;

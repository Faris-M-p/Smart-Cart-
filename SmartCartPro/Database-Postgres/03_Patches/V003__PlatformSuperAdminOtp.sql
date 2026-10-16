-- =============================================================================
-- V003 | Platform Super Admin flag + OTP challenges
-- =============================================================================
\set ON_ERROR_STOP on

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS is_platform_super_admin BOOLEAN NOT NULL DEFAULT FALSE;

CREATE INDEX IF NOT EXISTS ix_users_platform_super_admin
    ON users (is_platform_super_admin)
    WHERE cancelled = FALSE AND is_platform_super_admin = TRUE;

CREATE TABLE IF NOT EXISTS auth_otp_challenges (
    id_authotpchallenge INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    challengetoken  TEXT NOT NULL,
    fk_user         INT NOT NULL,
    otphash         TEXT NOT NULL,
    rememberme      BOOLEAN NOT NULL DEFAULT FALSE,
    attemptcount    INT NOT NULL DEFAULT 0,
    maxattempts     INT NOT NULL DEFAULT 5,
    expiresat       TIMESTAMP NOT NULL,
    consumedat      TIMESTAMP NULL,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_auth_otp_challenges_token UNIQUE (challengetoken),
    CONSTRAINT fk_auth_otp_challenges_user
        FOREIGN KEY (fk_user) REFERENCES users (id_user)
);

CREATE INDEX IF NOT EXISTS ix_auth_otp_challenges_fk_user ON auth_otp_challenges (fk_user);
CREATE INDEX IF NOT EXISTS ix_auth_otp_challenges_expiresat ON auth_otp_challenges (expiresat);

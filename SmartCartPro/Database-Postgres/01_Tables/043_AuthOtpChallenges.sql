-- =============================================================================
-- SmartCartPro | 043_AuthOtpChallenges.sql
-- Scope: Platform auth (Super Admin email OTP)
-- =============================================================================

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

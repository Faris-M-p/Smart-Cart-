-- One active sign-in per user. A new login increments session_version.
-- Cookies that still carry the previous number are rejected.
\set ON_ERROR_STOP on

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS session_version INT NOT NULL DEFAULT 1;

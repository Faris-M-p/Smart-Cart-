-- =============================================================================
-- Optional development trial license. Manual-only through RunDummyData.bat.
-- Idempotent: inserts only if no license exists.
-- =============================================================================
\set ON_ERROR_STOP on

-- Insert a 1-year active trial license only if the license table is empty.
INSERT INTO license (licensekey, license_type, status, startdate, expirydate, maxstores, notes, createdat)
SELECT
    'TRIAL-DEV-' || TO_CHAR(NOW(), 'YYYYMMDD'),
    'trial',
    'active',
    CURRENT_DATE,
    CURRENT_DATE + INTERVAL '1 year',
    5,
    'Optional development-only trial license. Never use this key in production.',
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM license WHERE cancelled = FALSE
);

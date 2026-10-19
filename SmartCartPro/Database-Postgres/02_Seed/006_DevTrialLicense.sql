-- =============================================================================
-- SmartCartPro | Seed V006 — Development / Trial License
-- Applied by: BuildDatabase.bat (02_Seed phase) or manually.
-- Idempotent — safe to rerun (inserts only if no license exists).
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
    'Auto-seeded development trial license. Replace with a production key before go-live.',
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM license WHERE cancelled = FALSE
);

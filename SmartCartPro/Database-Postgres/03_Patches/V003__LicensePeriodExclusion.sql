-- =============================================================================
-- SmartCartPro | V003__LicensePeriodExclusion.sql
-- Description: Prevent inclusive validity periods from overlapping.
-- Existing conflicts are reported and must be resolved manually before retry.
-- Review each reported pair against the original issuance records; manually
-- correct an erroneous boundary or cancel only a license confirmed as invalid,
-- with an auditable reason. Do not delete rows or make automatic adjustments.
-- =============================================================================

BEGIN;

DO $$
DECLARE
    conflict_summary TEXT;
BEGIN
    SELECT string_agg(
        format('%s (%s - %s) overlaps %s (%s - %s)',
            a.licensekey, to_char(a.startdate, 'DD Mon YYYY'), to_char(a.expirydate, 'DD Mon YYYY'),
            b.licensekey, to_char(b.startdate, 'DD Mon YYYY'), to_char(b.expirydate, 'DD Mon YYYY')),
        E'\n')
    INTO conflict_summary
    FROM license a
    JOIN license b
      ON a.id_license < b.id_license
     AND a.cancelled = FALSE
     AND b.cancelled = FALSE
     AND daterange(a.startdate, a.expirydate, '[]') && daterange(b.startdate, b.expirydate, '[]');

    IF conflict_summary IS NOT NULL THEN
        RAISE EXCEPTION 'Cannot enforce license period exclusion. Resolve these overlaps manually, preserving all records: %', conflict_summary;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'ex_license_nonoverlap'
          AND conrelid = 'license'::regclass
    ) THEN
        ALTER TABLE license
            ADD CONSTRAINT ex_license_nonoverlap EXCLUDE USING gist
            (daterange(startdate, expirydate, '[]') WITH &&)
            WHERE (cancelled = FALSE);
    END IF;
END $$;

INSERT INTO patch_history (version, description, appliedby, success)
VALUES ('V003', 'License validity period exclusion constraint', CURRENT_USER, TRUE)
ON CONFLICT (version) DO NOTHING;

COMMIT;

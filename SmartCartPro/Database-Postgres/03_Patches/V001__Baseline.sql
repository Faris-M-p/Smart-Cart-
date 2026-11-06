-- SmartCartPro first-release baseline.
-- 01_Tables contains final authoritative DDL; 02_Seed contains required system data.
-- Optional business/demo data is never included.
\set ON_ERROR_STOP on

BEGIN;
\ir ../01_Tables/create.sql
\ir ../02_Seed/create.sql
\ir ../DatabaseVersion.sql

INSERT INTO patch_history (version, description, appliedby, success)
VALUES ('V001', 'SmartCartPro baseline V001', current_user, TRUE);
COMMIT;

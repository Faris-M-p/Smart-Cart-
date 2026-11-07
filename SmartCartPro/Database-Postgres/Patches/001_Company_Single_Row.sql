-- Enforce the one-company-per-database rule for every company row.
-- This intentionally fails if the database already contains multiple rows;
-- inspect and resolve existing data manually before applying. No rows are changed.
CREATE UNIQUE INDEX IF NOT EXISTS uq_company_single_row
    ON company ((TRUE));

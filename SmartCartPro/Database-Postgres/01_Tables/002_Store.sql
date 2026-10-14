-- =============================================================================
-- SmartCartPro | 002_Store.sql
-- Scope: Company-wide (store master; each row is a store)
-- =============================================================================

-- Invariants (SQL + application):
-- 1. Store codes unique within company (uq_store_company_storecode).
-- 2. At most one default store per company (uq_store_one_default).
-- 3. Exactly one default store after initial setup: enforced in app transaction
--    that creates company + first store with isdefault=TRUE in one transaction.
-- 4. Do not deactivate/cancel the default store unless another active store is
--    first promoted to default (application transaction; not a trigger).
-- 5. Prevent multiple defaults via partial unique index below.

CREATE TABLE IF NOT EXISTS store (
    id_store        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fk_company      INT NOT NULL,
    storecode       TEXT NOT NULL,
    storename       TEXT NOT NULL,
    email           TEXT NULL,
    phone           TEXT NULL,
    addressline1    TEXT NULL,
    addressline2    TEXT NULL,
    city            TEXT NULL,
    state           TEXT NULL,
    postalcode      TEXT NULL,
    country         TEXT NULL,
    isdefault       BOOLEAN NOT NULL DEFAULT FALSE,
    isactive        BOOLEAN NOT NULL DEFAULT TRUE,
    createdat       TIMESTAMP NOT NULL DEFAULT NOW(),
    updatedat       TIMESTAMP NULL,
    cancelled       BOOLEAN NOT NULL DEFAULT FALSE,
    cancelledon     TIMESTAMP NULL,
    cancelledreason TEXT NULL,
    CONSTRAINT fk_store_company
        FOREIGN KEY (fk_company) REFERENCES company (id_company),
    CONSTRAINT uq_store_company_storecode UNIQUE (fk_company, storecode),
    CONSTRAINT ck_store_default_active
        CHECK (isdefault = FALSE OR (isactive = TRUE AND cancelled = FALSE))
);

-- Exactly one default among non-cancelled stores per company (at most one).
CREATE UNIQUE INDEX IF NOT EXISTS uq_store_one_default
    ON store (fk_company)
    WHERE isdefault = TRUE AND cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_store_fk_company ON store (fk_company);
CREATE INDEX IF NOT EXISTS ix_store_isactive ON store (isactive) WHERE cancelled = FALSE;

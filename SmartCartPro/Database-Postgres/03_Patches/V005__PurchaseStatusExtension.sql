-- =============================================================================
-- SmartCartPro | V005__PurchaseStatusExtension.sql
-- Adds: purchases.status values for draft/partially_received/received/cancelled
--       beyond the existing default 'posted'.
--       Uses no-op ALTER since PostgreSQL TEXT columns have no CHECK on status yet —
--       adding explicit CHECK for documentation/future enforcement.
-- =============================================================================

-- Add CHECK constraint to purchases.status to document valid values.
-- (Idempotent: wrapped in DO block to skip if constraint already exists.)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM   information_schema.table_constraints
        WHERE  table_name       = 'purchases'
          AND  constraint_name  = 'ck_purchases_status'
    ) THEN
        ALTER TABLE purchases
            ADD CONSTRAINT ck_purchases_status
            CHECK (status IN ('draft','posted','partially_received','received','cancelled'));
    END IF;
END;
$$;

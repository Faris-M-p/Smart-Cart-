-- =============================================================================
-- SmartCartPro | V002__DashboardPerformanceIndexes.sql
-- Description: Composite and filtered indexes for sub-second dashboard analytics.
-- =============================================================================

BEGIN;

CREATE INDEX IF NOT EXISTS ix_inventory_store_reorder
    ON inventory (fk_store, quantityonhand, reorderlevel);

CREATE INDEX IF NOT EXISTS ix_sales_returns_store_returndate
    ON sales_returns (fk_store, returndate)
    WHERE cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_purchases_store_purchasedate
    ON purchases (fk_store, purchasedate)
    WHERE cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_online_orders_store_orderdate
    ON online_orders (fk_store, orderdate)
    WHERE cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_cash_sessions_store_status_opened
    ON cash_sessions (fk_store, status, openedat DESC);

INSERT INTO patch_history (version, description, appliedby, success)
VALUES ('V002', 'Dashboard analytics performance indexes', CURRENT_USER, TRUE)
ON CONFLICT (version) DO NOTHING;

COMMIT;

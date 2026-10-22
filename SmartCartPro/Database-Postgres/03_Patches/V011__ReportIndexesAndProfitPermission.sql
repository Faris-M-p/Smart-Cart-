-- =============================================================================
-- V011__ReportIndexesAndProfitPermission.sql
-- Report indexes (no new business tables) and reports.profit permission.
-- Idempotent. ApplyPatches records patch_history after this file succeeds.
-- =============================================================================
\set ON_ERROR_STOP on

CREATE INDEX IF NOT EXISTS ix_sales_store_saledate ON sales (fk_store, saledate);
CREATE INDEX IF NOT EXISTS ix_sales_createdby ON sales (createdby);
CREATE INDEX IF NOT EXISTS ix_sales_status ON sales (status) WHERE cancelled = FALSE;
CREATE INDEX IF NOT EXISTS ix_sale_details_product_variant ON sale_details (fk_product, fk_productvariant);
CREATE INDEX IF NOT EXISTS ix_payments_cashsession ON payments (fk_cashsession);
CREATE INDEX IF NOT EXISTS ix_payments_store_paidat ON payments (fk_store, paidat);
CREATE INDEX IF NOT EXISTS ix_sales_returns_store_date ON sales_returns (fk_store, returndate);
CREATE INDEX IF NOT EXISTS ix_sales_return_details_product ON sales_return_details (fk_product, fk_productvariant);
CREATE INDEX IF NOT EXISTS ix_stock_movements_store_product_created ON stock_movements (fk_store, fk_product, createdat);
CREATE INDEX IF NOT EXISTS ix_stock_movements_type ON stock_movements (movementtype);
CREATE INDEX IF NOT EXISTS ix_purchases_store_date ON purchases (fk_store, purchasedate);
CREATE INDEX IF NOT EXISTS ix_purchase_details_product ON purchase_details (fk_product, fk_productvariant);
CREATE INDEX IF NOT EXISTS ix_cash_sessions_store_opened ON cash_sessions (fk_store, openedat);

INSERT INTO permissions (fk_module, permissioncode, permissionname, displayorder, isactive)
SELECT m.id_module, 'reports.profit', 'View cost and profit on reports', 2, TRUE
FROM modules m
WHERE m.modulecode = 'REPORTS' AND m.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM permissions p
      WHERE p.permissioncode = 'reports.profit' AND p.cancelled = FALSE
  );

INSERT INTO role_permissions (fk_role, fk_permission)
SELECT r.id_role, p.id_permission
FROM roles r
CROSS JOIN permissions p
WHERE r.rolecode IN ('ADMIN', 'SUPER_ADMIN')
  AND r.cancelled = FALSE
  AND p.permissioncode = 'reports.profit'
  AND p.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM role_permissions rp
      WHERE rp.fk_role = r.id_role AND rp.fk_permission = p.id_permission
  );

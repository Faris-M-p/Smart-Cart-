-- =============================================================================
-- V006__PosCancelPermission.sql
-- Adds pos.cancel permission (invoice cancel) and grants to the company Admin role.
-- Idempotent.
-- =============================================================================
\set ON_ERROR_STOP on

INSERT INTO permissions (fk_module, permissioncode, permissionname, displayorder, isactive)
SELECT m.id_module, 'pos.cancel', 'Cancel POS invoices', 3, TRUE
FROM modules m
WHERE m.modulecode = 'POS' AND m.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM permissions p
      WHERE p.permissioncode = 'pos.cancel' AND p.cancelled = FALSE
  );

INSERT INTO role_permissions (fk_role, fk_permission)
SELECT r.id_role, p.id_permission
FROM roles r
CROSS JOIN permissions p
WHERE r.rolecode IN ('ADMIN', 'SUPER_ADMIN')
  AND r.cancelled = FALSE
  AND p.permissioncode = 'pos.cancel'
  AND p.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM role_permissions rp
      WHERE rp.fk_role = r.id_role AND rp.fk_permission = p.id_permission
  );

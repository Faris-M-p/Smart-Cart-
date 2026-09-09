\echo 'Seeding Admin role permission mappings...'

INSERT INTO user_role_permissions (fk_user_role, fk_permission, created_at, cancelled)
SELECT r.id_user_role, p.id_permission, NOW(), FALSE
FROM user_roles r
CROSS JOIN permissions p
WHERE r.role_name = 'Admin'
  AND r.cancelled = FALSE
  AND p.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1
      FROM user_role_permissions urp
      WHERE urp.fk_user_role = r.id_user_role
        AND urp.fk_permission = p.id_permission
  );

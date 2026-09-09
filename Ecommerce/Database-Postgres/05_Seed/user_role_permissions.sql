\echo 'Seeding Admin role permission mappings...'

INSERT INTO userrolepermissions (fk_userrole, fk_permission, createdat, cancelled)
SELECT r.id_userrole, p.id_permission, NOW(), FALSE
FROM userroles r
CROSS JOIN permissions p
WHERE r.rolename = 'Admin'
  AND r.cancelled = FALSE
  AND p.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1
      FROM userrolepermissions urp
      WHERE urp.fk_userrole = r.id_userrole
        AND urp.fk_permission = p.id_permission
  );

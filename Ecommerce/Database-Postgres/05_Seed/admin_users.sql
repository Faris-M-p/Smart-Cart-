\echo 'Seeding initial Admin user...'

/*
    Temporary development credentials:
      UserName : admin
      Password : Admin@123
    PasswordHash is ASP.NET Core Identity V3 (PBKDF2-HMAC-SHA256).
*/
INSERT INTO adminusers (
    fk_userrole, username, passwordhash, fullname, email,
    phonenumber, isactive, createdat, cancelled
)
SELECT
    r.id_userrole,
    'admin',
    'AQAAAAEAAYagAAAAEPGBDPr1vrIh1zlYcblDhIRjePewKQpikuv9yxb2oVFsGwoDVTS9GhHDPlcaMiPGEA==',
    'System Administrator',
    'admin@smartcart.local',
    NULL,
    TRUE,
    NOW(),
    FALSE
FROM userroles r
WHERE r.rolename = 'Admin'
  AND r.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM adminusers WHERE username = 'admin'
  );

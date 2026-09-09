\echo 'Seeding initial Admin user...'

/*
    Temporary development credentials:
      UserName : admin
      Password : Admin@123
    PasswordHash is ASP.NET Core Identity V3 (PBKDF2-HMAC-SHA256).
*/
INSERT INTO admin_users (
    fk_user_role, user_name, password_hash, full_name, email,
    phone_number, is_active, created_at, cancelled
)
SELECT
    r.id_user_role,
    'admin',
    'AQAAAAEAAYagAAAAEPGBDPr1vrIh1zlYcblDhIRjePewKQpikuv9yxb2oVFsGwoDVTS9GhHDPlcaMiPGEA==',
    'System Administrator',
    'admin@smartcart.local',
    NULL,
    TRUE,
    NOW(),
    FALSE
FROM user_roles r
WHERE r.role_name = 'Admin'
  AND r.cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM admin_users WHERE user_name = 'admin'
  );

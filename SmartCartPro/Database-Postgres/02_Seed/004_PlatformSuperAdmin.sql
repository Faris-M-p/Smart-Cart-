-- =============================================================================
-- SmartCartPro | 004_PlatformSuperAdmin.sql
-- Platform Super Admin seed credential (idempotent)
-- =============================================================================
-- Email: Muhdfarismp@gmail.com
-- Password: Faris@8816 (ASP.NET Identity PasswordHasher V3 hash below)
-- Note: is_platform_super_admin grants platform super admin authorization.
-- =============================================================================

INSERT INTO users (
    username,
    email,
    passwordhash,
    fullname,
    isactive,
    mustchangepassword,
    is_platform_super_admin,
    createdat
)
SELECT
    'Muhdfarismp@gmail.com',
    'Muhdfarismp@gmail.com',
    'AQAAAAIAAYagAAAAEGBtvSNJmffomee/xytczFFkIFuNPWWCoWTzC3wrHtAVr5iDiH++s++D8ytpJYYPWg==',
    'Platform Super Admin',
    TRUE,
    FALSE,
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM users u
    WHERE LOWER(u.email) = LOWER('Muhdfarismp@gmail.com')
      AND u.cancelled = FALSE
);

-- Ensure flag and password if the email already exists
UPDATE users
SET is_platform_super_admin = TRUE,
    passwordhash = 'AQAAAAIAAYagAAAAEGBtvSNJmffomee/xytczFFkIFuNPWWCoWTzC3wrHtAVr5iDiH++s++D8ytpJYYPWg==',
    isactive = TRUE,
    mustchangepassword = FALSE,
    updatedat = NOW()
WHERE LOWER(email) = LOWER('Muhdfarismp@gmail.com')
  AND cancelled = FALSE;


-- Platform Super Admin seed (idempotent)
-- Login email: Muhdfarismp@gmail.com
-- Dev password: Admin@123 (ASP.NET Identity PasswordHasher V3 hash below)
-- Company Employees UI must never expose/edit is_platform_super_admin.
\set ON_ERROR_STOP on

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
    'AQAAAAIAAYagAAAAEMwD8hjWbMf/QYpdRJNr7Lh4y+wGZTz0+4bgKBektuHXhV1ZqgL/KONn5CMWhSCZXQ==',
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

-- Ensure flag if the email already existed without platform rights
UPDATE users
SET is_platform_super_admin = TRUE,
    updatedat = NOW()
WHERE LOWER(email) = LOWER('Muhdfarismp@gmail.com')
  AND cancelled = FALSE
  AND is_platform_super_admin = FALSE;

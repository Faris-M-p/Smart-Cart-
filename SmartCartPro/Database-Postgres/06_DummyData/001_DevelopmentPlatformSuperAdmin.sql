-- Optional development-only Platform Super Admin account.
-- Never run on a production database. This account has a hard-coded demo credential.
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
    TRUE,
    TRUE,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM users u
    WHERE LOWER(u.email) = LOWER('Muhdfarismp@gmail.com')
      AND u.cancelled = FALSE
);

-- Existing users are never modified: this inserts only when the email is unused.

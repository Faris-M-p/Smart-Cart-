\echo 'Seeding user_roles...'

INSERT INTO user_roles (
    role_name, description, is_system_role, is_active, created_at, cancelled
)
SELECT
    'Admin',
    'System Administrator with complete access',
    TRUE,
    TRUE,
    NOW(),
    FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM user_roles WHERE role_name = 'Admin'
);

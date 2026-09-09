\echo 'Seeding user_roles...'

INSERT INTO userroles (
    rolename, description, issystemrole, isactive, createdat, cancelled
)
SELECT
    'Admin',
    'System Administrator with complete access',
    TRUE,
    TRUE,
    NOW(),
    FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM userroles WHERE rolename = 'Admin'
);

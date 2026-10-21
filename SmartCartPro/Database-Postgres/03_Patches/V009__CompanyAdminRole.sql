-- Company role is Admin. The software provider remains users.is_platform_super_admin.
-- Renames the existing system row in place so current assignments keep the same id.
\set ON_ERROR_STOP on

UPDATE roles
SET rolecode = 'ADMIN',
    rolename = 'Admin',
    description = 'Company administrator. Uses every module the software provider has enabled, and manages employees and roles.',
    updatedat = NOW()
WHERE rolecode = 'SUPER_ADMIN'
  AND cancelled = FALSE
  AND NOT EXISTS (
      SELECT 1 FROM roles existing
      WHERE existing.rolecode = 'ADMIN'
        AND existing.cancelled = FALSE
  );

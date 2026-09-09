/* =============================================================================
   05_Seed / patch.sql
   -----------------------------------------------------------------------------
   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\i ./05_Seed/modules.sql
\echo 'modules patch successfully completed.'

\i ./05_Seed/permissions.sql
\echo 'permissions patch successfully completed.'

\i ./05_Seed/user_roles.sql
\echo 'user_roles patch successfully completed.'

\i ./05_Seed/user_role_permissions.sql
\echo 'user_role_permissions patch successfully completed.'

\i ./05_Seed/admin_users.sql
\echo 'admin_users patch successfully completed.'

\i ./05_Seed/super_market_catalog.sql
\echo 'super_market_catalog patch successfully completed.'

/* Development electronics sample (commented — convert from SQL Server before use):
   -- \i ./05_Seed/dev_sample_catalog.sql
*/

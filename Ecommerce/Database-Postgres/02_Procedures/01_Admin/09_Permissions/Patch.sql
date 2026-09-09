/* =============================================================================
   01_Admin / 09_Permissions / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_permission_tree_select ---'
\i ./02_Procedures/01_Admin/09_Permissions/pro_permission_tree_select.sql
\echo 'pro_permission_tree_select patch successfully completed.'

\echo '--- pro_user_role_permission_select ---'
\i ./02_Procedures/01_Admin/09_Permissions/pro_user_role_permission_select.sql
\echo 'pro_user_role_permission_select patch successfully completed.'

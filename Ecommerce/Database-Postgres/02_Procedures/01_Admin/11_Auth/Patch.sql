/* =============================================================================
   01_Admin / 11_Auth / Patch.sql
   -----------------------------------------------------------------------------
   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_admin_auth_select_by_user_name ---'
\i ./02_Procedures/01_Admin/11_Auth/pro_admin_auth_select_by_user_name.sql
\echo 'pro_admin_auth_select_by_user_name patch successfully completed.'

\echo '--- pro_admin_auth_permission_select ---'
\i ./02_Procedures/01_Admin/11_Auth/pro_admin_auth_permission_select.sql
\echo 'pro_admin_auth_permission_select patch successfully completed.'

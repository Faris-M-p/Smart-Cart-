/* =============================================================================
   01_Admin / 08_UserRoles / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_user_role_save ---'
\i ./02_Procedures/01_Admin/08_UserRoles/pro_user_role_save.sql
\echo 'pro_user_role_save patch successfully completed.'

\echo '--- pro_user_role_list_select ---'
\i ./02_Procedures/01_Admin/08_UserRoles/pro_user_role_list_select.sql
\echo 'pro_user_role_list_select patch successfully completed.'

\echo '--- pro_user_role_select_by_id ---'
\i ./02_Procedures/01_Admin/08_UserRoles/pro_user_role_select_by_id.sql
\echo 'pro_user_role_select_by_id patch successfully completed.'

\echo '--- pro_user_role_cancel ---'
\i ./02_Procedures/01_Admin/08_UserRoles/pro_user_role_cancel.sql
\echo 'pro_user_role_cancel patch successfully completed.'

/* =============================================================================
   01_Admin / 10_Employees / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_employee_insert ---'
\i ./02_Procedures/01_Admin/10_Employees/pro_employee_insert.sql
\echo 'pro_employee_insert patch successfully completed.'

\echo '--- pro_employee_update ---'
\i ./02_Procedures/01_Admin/10_Employees/pro_employee_update.sql
\echo 'pro_employee_update patch successfully completed.'

\echo '--- pro_employee_delete ---'
\i ./02_Procedures/01_Admin/10_Employees/pro_employee_delete.sql
\echo 'pro_employee_delete patch successfully completed.'

\echo '--- pro_employee_list_select ---'
\i ./02_Procedures/01_Admin/10_Employees/pro_employee_list_select.sql
\echo 'pro_employee_list_select patch successfully completed.'

\echo '--- pro_employee_select_by_id ---'
\i ./02_Procedures/01_Admin/10_Employees/pro_employee_select_by_id.sql
\echo 'pro_employee_select_by_id patch successfully completed.'

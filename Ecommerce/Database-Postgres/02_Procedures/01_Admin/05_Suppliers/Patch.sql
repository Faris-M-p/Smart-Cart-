/* =============================================================================
   01_Admin / 05_Suppliers / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_supplier_update ---'
\i ./02_Procedures/01_Admin/05_Suppliers/pro_supplier_update.sql
\echo 'pro_supplier_update patch successfully completed.'

\echo '--- pro_supplier_list_select ---'
\i ./02_Procedures/01_Admin/05_Suppliers/pro_supplier_list_select.sql
\echo 'pro_supplier_list_select patch successfully completed.'

\echo '--- pro_supplier_select_by_id ---'
\i ./02_Procedures/01_Admin/05_Suppliers/pro_supplier_select_by_id.sql
\echo 'pro_supplier_select_by_id patch successfully completed.'

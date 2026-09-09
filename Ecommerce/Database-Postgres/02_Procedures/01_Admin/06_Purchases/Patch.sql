/* =============================================================================
   01_Admin / 06_Purchases / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_purchase_update ---'
\i ./02_Procedures/01_Admin/06_Purchases/pro_purchase_update.sql
\echo 'pro_purchase_update patch successfully completed.'

\echo '--- pro_purchase_list_select ---'
\i ./02_Procedures/01_Admin/06_Purchases/pro_purchase_list_select.sql
\echo 'pro_purchase_list_select patch successfully completed.'

\echo '--- pro_purchase_detail_select ---'
\i ./02_Procedures/01_Admin/06_Purchases/pro_purchase_detail_select.sql
\echo 'pro_purchase_detail_select patch successfully completed.'

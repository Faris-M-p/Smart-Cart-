/* =============================================================================
   01_Admin / 03_Categories / Patch.sql
   -----------------------------------------------------------------------------
   Controls which category and subcategory procedure scripts execute.
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_category_update ---'
\i ./02_Procedures/01_Admin/03_Categories/pro_category_update.sql
\echo 'pro_category_update patch successfully completed.'

\echo '--- pro_category_list_select ---'
\i ./02_Procedures/01_Admin/03_Categories/pro_category_list_select.sql
\echo 'pro_category_list_select patch successfully completed.'

\echo '--- pro_category_delete ---'
\i ./02_Procedures/01_Admin/03_Categories/pro_category_delete.sql
\echo 'pro_category_delete patch successfully completed.'

\echo '--- pro_sub_category_update ---'
\i ./02_Procedures/01_Admin/03_Categories/pro_sub_category_update.sql
\echo 'pro_sub_category_update patch successfully completed.'

\echo '--- pro_sub_category_list_select ---'
\i ./02_Procedures/01_Admin/03_Categories/pro_sub_category_list_select.sql
\echo 'pro_sub_category_list_select patch successfully completed.'

\echo '--- pro_sub_category_delete ---'
\i ./02_Procedures/01_Admin/03_Categories/pro_sub_category_delete.sql
\echo 'pro_sub_category_delete patch successfully completed.'

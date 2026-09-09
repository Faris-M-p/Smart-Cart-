/* =============================================================================
   01_Admin / 02_Brands / Patch.sql
   -----------------------------------------------------------------------------
   Controls which brand procedure scripts execute.
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_brand_update ---'
\i ./02_Procedures/01_Admin/02_Brands/pro_brand_update.sql
\echo 'pro_brand_update patch successfully completed.'

\echo '--- pro_brand_list_select ---'
\i ./02_Procedures/01_Admin/02_Brands/pro_brand_list_select.sql
\echo 'pro_brand_list_select patch successfully completed.'

\echo '--- pro_brand_select_by_id ---'
\i ./02_Procedures/01_Admin/02_Brands/pro_brand_select_by_id.sql
\echo 'pro_brand_select_by_id patch successfully completed.'

\echo '--- pro_brand_delete ---'
\i ./02_Procedures/01_Admin/02_Brands/pro_brand_delete.sql
\echo 'pro_brand_delete patch successfully completed.'

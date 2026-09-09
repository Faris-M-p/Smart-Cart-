/* =============================================================================
   01_Admin / 01_Products / Patch.sql
   -----------------------------------------------------------------------------
   Controls which product / sku procedure scripts execute.
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_product_update ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_update.sql
\echo 'pro_product_update patch successfully completed.'

\echo '--- pro_product_list_select ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_list_select.sql
\echo 'pro_product_list_select patch successfully completed.'

\echo '--- pro_product_select_by_id ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_select_by_id.sql
\echo 'pro_product_select_by_id patch successfully completed.'

\echo '--- pro_product_delete ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_delete.sql
\echo 'pro_product_delete patch successfully completed.'

\echo '--- pro_product_detail_select ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_detail_select.sql
\echo 'pro_product_detail_select patch successfully completed.'

\echo '--- pro_product_image_update ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_image_update.sql
\echo 'pro_product_image_update patch successfully completed.'

\echo '--- pro_product_variant_upsert ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_variant_upsert.sql
\echo 'pro_product_variant_upsert patch successfully completed.'

\echo '--- pro_product_variant_select ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_variant_select.sql
\echo 'pro_product_variant_select patch successfully completed.'

\echo '--- pro_product_variant_image_update ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_variant_image_update.sql
\echo 'pro_product_variant_image_update patch successfully completed.'

\echo '--- pro_product_variant_image_select ---'
\i ./02_Procedures/01_Admin/01_Products/pro_product_variant_image_select.sql
\echo 'pro_product_variant_image_select patch successfully completed.'

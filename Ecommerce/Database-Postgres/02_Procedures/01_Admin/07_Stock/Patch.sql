/* =============================================================================
   01_Admin / 07_Stock / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- pro_product_variant_stock_upsert ---'
\i ./02_Procedures/01_Admin/07_Stock/pro_product_variant_stock_upsert.sql
\echo 'pro_product_variant_stock_upsert patch successfully completed.'

\echo '--- pro_product_variant_stock_select ---'
\i ./02_Procedures/01_Admin/07_Stock/pro_product_variant_stock_select.sql
\echo 'pro_product_variant_stock_select patch successfully completed.'

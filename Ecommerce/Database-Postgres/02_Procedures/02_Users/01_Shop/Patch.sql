/* =============================================================================
   02_Users / 01_Shop / Patch.sql
   -----------------------------------------------------------------------------
   Controls which storefront catalog procedure scripts execute.
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\i ./02_Procedures/02_Users/01_Shop/get_products.sql
\echo 'get_products patch successfully completed.'

\i ./02_Procedures/02_Users/01_Shop/get_product_details.sql
\echo 'get_product_details patch successfully completed.'

\i ./02_Procedures/02_Users/01_Shop/get_shop_filters.sql
\echo 'get_shop_filters patch successfully completed.'

\i ./02_Procedures/02_Users/01_Shop/get_product_details_by_id.sql
\echo 'get_product_details_by_id patch successfully completed.'

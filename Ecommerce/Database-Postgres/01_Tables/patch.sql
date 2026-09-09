/* =============================================================================
   01_Tables / patch.sql — incremental (idempotent CREATE IF NOT EXISTS)
   For Postgres full builds, create.sql already has final schema folded in.
   Patch re-includes media tables + ensures StoreSettings cleanup.
   ============================================================================= */

\echo '--- product_media (idempotent) ---'
\i ./01_Tables/product_media.sql
\echo 'product_media completed.'

\echo '--- sku_media (idempotent) ---'
\i ./01_Tables/sku_media.sql
\echo 'sku_media completed.'

\echo '--- sales (idempotent) ---'
\i ./01_Tables/sales.sql
\echo 'sales completed.'

\echo '--- sales_detail (idempotent) ---'
\i ./01_Tables/sales_detail.sql
\echo 'sales_detail completed.'

\echo '--- sales_return (idempotent) ---'
\i ./01_Tables/sales_return.sql
\echo 'sales_return completed.'

\echo '--- sales_return_detail (idempotent) ---'
\i ./01_Tables/sales_return_detail.sql
\echo 'sales_return_detail completed.'

\echo '--- drop legacy store_settings if present ---'
DROP PROCEDURE IF EXISTS get_store_settings;
DROP PROCEDURE IF EXISTS update_store_settings;
DROP TABLE IF EXISTS store_settings CASCADE;
\echo 'store_settings removal completed.'

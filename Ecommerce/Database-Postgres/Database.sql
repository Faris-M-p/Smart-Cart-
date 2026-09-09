/* =============================================================================
   SmartCart - Master PostgreSQL database build script
   -----------------------------------------------------------------------------
   Run with psql from this folder (Database-Postgres):

     psql -v ON_ERROR_STOP=1 -v dbname=smartcart -f Database.sql

   Optional: set dbname (default smartcart). Connects first to maintenance DB
   "postgres", creates target DB if missing, then \c into it.

   Flow:
     Database.sql
         → 01_Tables/create.sql
         → 01_Tables/patch.sql
         → 05_Seed/patch.sql
         → each procedure module Patch.sql
   ============================================================================= */

\set ON_ERROR_STOP on
\if :{?dbname}
\else
\set dbname smartcart
\endif

\echo '=== SmartCart PostgreSQL database build starting ==='
\echo 'Target database:' :dbname

\c postgres

SELECT format('CREATE DATABASE %I', :'dbname')
WHERE NOT EXISTS (
    SELECT 1 FROM pg_database WHERE datname = :'dbname'
)\gexec

\echo 'Connecting to target database...'
\c :dbname

\echo '--- Creating tables ---'
\i ./01_Tables/create.sql
\echo 'Tables create module successfully completed.'

\echo '--- Applying table patches ---'
\i ./01_Tables/patch.sql
\echo 'Tables patch module successfully completed.'

\echo '--- Seeding authentication data ---'
\i ./05_Seed/patch.sql
\echo 'Seed patch module successfully completed.'

\echo '--- Functions (none) ---'

\echo '--- Creating stored procedures ---'

\echo '--- Admin / Products ---'
\i ./02_Procedures/01_Admin/01_Products/Patch.sql
\echo 'Admin / Products patch module successfully completed.'

\echo '--- Admin / Brands ---'
\i ./02_Procedures/01_Admin/02_Brands/Patch.sql
\echo 'Admin / Brands patch module successfully completed.'

\echo '--- Admin / Categories ---'
\i ./02_Procedures/01_Admin/03_Categories/Patch.sql
\echo 'Admin / Categories patch module successfully completed.'

\echo '--- Admin / Variants ---'
\i ./02_Procedures/01_Admin/04_Variants/Patch.sql
\echo 'Admin / Variants patch module successfully completed.'

\echo '--- Admin / Suppliers ---'
\i ./02_Procedures/01_Admin/05_Suppliers/Patch.sql
\echo 'Admin / Suppliers patch module successfully completed.'

\echo '--- Admin / Purchases ---'
\i ./02_Procedures/01_Admin/06_Purchases/Patch.sql
\echo 'Admin / Purchases patch module successfully completed.'

\echo '--- Admin / Stock ---'
\i ./02_Procedures/01_Admin/07_Stock/Patch.sql
\echo 'Admin / Stock patch module successfully completed.'

\echo '--- Admin / UserRoles ---'
\i ./02_Procedures/01_Admin/08_UserRoles/Patch.sql
\echo 'Admin / UserRoles patch module successfully completed.'

\echo '--- Admin / Permissions ---'
\i ./02_Procedures/01_Admin/09_Permissions/Patch.sql
\echo 'Admin / Permissions patch module successfully completed.'

\echo '--- Admin / Employees ---'
\i ./02_Procedures/01_Admin/10_Employees/Patch.sql
\echo 'Admin / Employees patch module successfully completed.'

\echo '--- Admin / Auth ---'
\i ./02_Procedures/01_Admin/11_Auth/Patch.sql
\echo 'Admin / Auth patch module successfully completed.'

\echo '--- Admin / Orders ---'
\i ./02_Procedures/01_Admin/12_Orders/Patch.sql
\echo 'Admin / Orders patch module successfully completed.'

\echo '--- Users / Shop ---'
\i ./02_Procedures/02_Users/01_Shop/Patch.sql
\echo 'Users / Shop patch module successfully completed.'

\echo '--- Users / Cart ---'
\i ./02_Procedures/02_Users/02_Cart/Patch.sql
\echo 'Users / Cart patch module successfully completed.'

\echo '--- Users / Wishlist ---'
\i ./02_Procedures/02_Users/03_Wishlist/Patch.sql
\echo 'Users / Wishlist patch module successfully completed.'

\echo '--- Users / Auth ---'
\i ./02_Procedures/02_Users/04_Auth/Patch.sql
\echo 'Users / Auth patch module successfully completed.'

\echo '--- Users / Orders ---'
\i ./02_Procedures/02_Users/05_Orders/Patch.sql
\echo 'Users / Orders patch module successfully completed.'

\echo '--- Common / Stock ---'
\i ./02_Procedures/03_Common/01_Stock/Patch.sql
\echo 'Common / Stock patch module successfully completed.'

\echo '--- Views (none) ---'

\echo '=== SmartCart PostgreSQL database build completed successfully ==='

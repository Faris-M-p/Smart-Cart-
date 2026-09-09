/* =============================================================================
   03_Common / 01_Stock / Patch.sql
   -----------------------------------------------------------------------------
   Controls which shared stock procedure scripts execute.
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\i ./02_Procedures/03_Common/01_Stock/pro_stock_available_select.sql
\echo 'pro_stock_available_select patch successfully completed.'

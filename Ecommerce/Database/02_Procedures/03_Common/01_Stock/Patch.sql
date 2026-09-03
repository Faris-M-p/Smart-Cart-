/* =============================================================================
   03_Common / 01_Stock / Patch.sql
   -----------------------------------------------------------------------------
   Controls which shared stock procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\03_Common\01_Stock\ProStockAvailableSelect.sql

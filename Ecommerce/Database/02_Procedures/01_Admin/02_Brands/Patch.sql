/* =============================================================================
   01_Admin / 02_Brands / Patch.sql
   -----------------------------------------------------------------------------
   Controls which brand procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\02_Brands\ProBrandUpdate.sql
:r .\02_Procedures\01_Admin\02_Brands\ProBrandListSelect.sql
:r .\02_Procedures\01_Admin\02_Brands\ProBrandSelectById.sql
:r .\02_Procedures\01_Admin\02_Brands\ProBrandDelete.sql

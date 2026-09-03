/* =============================================================================
   02_Users / 01_Shop / Patch.sql
   -----------------------------------------------------------------------------
   Controls which storefront catalog procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\02_Users\01_Shop\GetProducts.sql
:r .\02_Procedures\02_Users\01_Shop\GetProductDetailsById.sql

/* =============================================================================
   01_Admin / 01_Products / Patch.sql
   -----------------------------------------------------------------------------
   Controls which product / SKU procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\01_Products\ProProductUpdate.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductListSelect.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductSelectById.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductDelete.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductDetailSelect.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductImageUpdate.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantUpsert.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantSelect.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantImageUpdate.sql
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantImageSelect.sql

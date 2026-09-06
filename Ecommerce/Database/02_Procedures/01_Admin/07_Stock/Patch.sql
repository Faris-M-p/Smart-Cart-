/* =============================================================================
   01_Admin / 07_Stock / Patch.sql
   -----------------------------------------------------------------------------
   Controls which admin inventory / SKU-stock procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\07_Stock\ProProductVariantStockUpsert.sql
GO
PRINT N'ProProductVariantStockUpsert patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\07_Stock\ProProductVariantStockSelect.sql
GO
PRINT N'ProProductVariantStockSelect patch successfully completed.';
GO

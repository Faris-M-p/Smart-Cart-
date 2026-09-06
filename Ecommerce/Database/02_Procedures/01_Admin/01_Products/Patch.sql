/* =============================================================================
   01_Admin / 01_Products / Patch.sql
   -----------------------------------------------------------------------------
   Controls which product / SKU procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\01_Products\ProProductUpdate.sql
GO
PRINT N'ProProductUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductListSelect.sql
GO
PRINT N'ProProductListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductSelectById.sql
GO
PRINT N'ProProductSelectById patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductDelete.sql
GO
PRINT N'ProProductDelete patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductDetailSelect.sql
GO
PRINT N'ProProductDetailSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductImageUpdate.sql
GO
PRINT N'ProProductImageUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantUpsert.sql
GO
PRINT N'ProProductVariantUpsert patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantSelect.sql
GO
PRINT N'ProProductVariantSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantImageUpdate.sql
GO
PRINT N'ProProductVariantImageUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\01_Products\ProProductVariantImageSelect.sql
GO
PRINT N'ProProductVariantImageSelect patch successfully completed.';
GO

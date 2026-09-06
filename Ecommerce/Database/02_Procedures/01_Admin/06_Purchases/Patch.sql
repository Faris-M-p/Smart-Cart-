/* =============================================================================
   01_Admin / 06_Purchases / Patch.sql
   -----------------------------------------------------------------------------
   Controls which purchase procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\06_Purchases\ProPurchaseUpdate.sql
GO
PRINT N'ProPurchaseUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\06_Purchases\ProPurchaseListSelect.sql
GO
PRINT N'ProPurchaseListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\06_Purchases\ProPurchaseDetailSelect.sql
GO
PRINT N'ProPurchaseDetailSelect patch successfully completed.';
GO

/* =============================================================================
   01_Admin / 05_Suppliers / Patch.sql
   -----------------------------------------------------------------------------
   Controls which supplier procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\05_Suppliers\ProSupplierUpdate.sql
GO
PRINT N'ProSupplierUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\05_Suppliers\ProSupplierListSelect.sql
GO
PRINT N'ProSupplierListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\05_Suppliers\ProSupplierSelectById.sql
GO
PRINT N'ProSupplierSelectById patch successfully completed.';
GO

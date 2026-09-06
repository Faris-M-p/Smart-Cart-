/* =============================================================================
   01_Admin / 02_Brands / Patch.sql
   -----------------------------------------------------------------------------
   Controls which brand procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\02_Brands\ProBrandUpdate.sql
GO
PRINT N'ProBrandUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\02_Brands\ProBrandListSelect.sql
GO
PRINT N'ProBrandListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\02_Brands\ProBrandSelectById.sql
GO
PRINT N'ProBrandSelectById patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\02_Brands\ProBrandDelete.sql
GO
PRINT N'ProBrandDelete patch successfully completed.';
GO

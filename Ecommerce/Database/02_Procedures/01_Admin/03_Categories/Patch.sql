/* =============================================================================
   01_Admin / 03_Categories / Patch.sql
   -----------------------------------------------------------------------------
   Controls which category and subcategory procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\03_Categories\ProCategoryUpdate.sql
GO
PRINT N'ProCategoryUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\03_Categories\ProCategoryListSelect.sql
GO
PRINT N'ProCategoryListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\03_Categories\ProCategoryDelete.sql
GO
PRINT N'ProCategoryDelete patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\03_Categories\ProSubCategoryUpdate.sql
GO
PRINT N'ProSubCategoryUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\03_Categories\ProSubCategoryListSelect.sql
GO
PRINT N'ProSubCategoryListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\03_Categories\ProSubCategoryDelete.sql
GO
PRINT N'ProSubCategoryDelete patch successfully completed.';
GO

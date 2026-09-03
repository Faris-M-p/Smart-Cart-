/* =============================================================================
   01_Admin / 03_Categories / Patch.sql
   -----------------------------------------------------------------------------
   Controls which category and subcategory procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\03_Categories\ProCategoryUpdate.sql
:r .\02_Procedures\01_Admin\03_Categories\ProCategoryListSelect.sql
:r .\02_Procedures\01_Admin\03_Categories\ProCategoryDelete.sql
:r .\02_Procedures\01_Admin\03_Categories\ProSubCategoryUpdate.sql
:r .\02_Procedures\01_Admin\03_Categories\ProSubCategoryListSelect.sql
:r .\02_Procedures\01_Admin\03_Categories\ProSubCategoryDelete.sql

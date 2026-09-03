/* =============================================================================
   01_Admin / 04_Variants / Patch.sql
   -----------------------------------------------------------------------------
   Controls which variant / variant-value master procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\04_Variants\ProVariantUpdate.sql
:r .\02_Procedures\01_Admin\04_Variants\ProVariantListSelect.sql
:r .\02_Procedures\01_Admin\04_Variants\ProVariantSelectById.sql
:r .\02_Procedures\01_Admin\04_Variants\ProVariantValueUpdate.sql
:r .\02_Procedures\01_Admin\04_Variants\ProVariantValueListSelect.sql
:r .\02_Procedures\01_Admin\04_Variants\ProVariantValueSelectById.sql

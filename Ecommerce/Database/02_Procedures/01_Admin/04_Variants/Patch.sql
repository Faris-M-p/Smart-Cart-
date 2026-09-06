/* =============================================================================
   01_Admin / 04_Variants / Patch.sql
   -----------------------------------------------------------------------------
   Controls which variant / variant-value master procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\04_Variants\ProVariantUpdate.sql
GO
PRINT N'ProVariantUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\04_Variants\ProVariantListSelect.sql
GO
PRINT N'ProVariantListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\04_Variants\ProVariantSelectById.sql
GO
PRINT N'ProVariantSelectById patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\04_Variants\ProVariantValueUpdate.sql
GO
PRINT N'ProVariantValueUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\04_Variants\ProVariantValueListSelect.sql
GO
PRINT N'ProVariantValueListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\04_Variants\ProVariantValueSelectById.sql
GO
PRINT N'ProVariantValueSelectById patch successfully completed.';
GO

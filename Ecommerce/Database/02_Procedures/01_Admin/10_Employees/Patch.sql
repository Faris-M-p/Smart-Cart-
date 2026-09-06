/* =============================================================================
   01_Admin / 10_Employees / Patch.sql
   -----------------------------------------------------------------------------
   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\10_Employees\ProEmployeeListSelect.sql
GO
PRINT N'ProEmployeeListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\10_Employees\ProEmployeeSelectById.sql
GO
PRINT N'ProEmployeeSelectById patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\10_Employees\ProEmployeeInsert.sql
GO
PRINT N'ProEmployeeInsert patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\10_Employees\ProEmployeeUpdate.sql
GO
PRINT N'ProEmployeeUpdate patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\10_Employees\ProEmployeeDelete.sql
GO
PRINT N'ProEmployeeDelete patch successfully completed.';
GO

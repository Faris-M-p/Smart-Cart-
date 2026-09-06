/* =============================================================================
   01_Admin / 09_Permissions / Patch.sql
   -----------------------------------------------------------------------------
   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\09_Permissions\ProPermissionTreeSelect.sql
GO
PRINT N'ProPermissionTreeSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\09_Permissions\ProUserRolePermissionSelect.sql
GO
PRINT N'ProUserRolePermissionSelect patch successfully completed.';
GO

/* =============================================================================
   01_Admin / 11_Auth / Patch.sql
   -----------------------------------------------------------------------------
   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\11_Auth\ProAdminAuthSelectByUserName.sql
GO
PRINT N'ProAdminAuthSelectByUserName patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\11_Auth\ProAdminAuthPermissionSelect.sql
GO
PRINT N'ProAdminAuthPermissionSelect patch successfully completed.';
GO

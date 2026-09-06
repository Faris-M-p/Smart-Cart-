/* =============================================================================
   01_Admin / 08_UserRoles / Patch.sql
   -----------------------------------------------------------------------------
   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\08_UserRoles\ProUserRoleListSelect.sql
GO
PRINT N'ProUserRoleListSelect patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\08_UserRoles\ProUserRoleSelectById.sql
GO
PRINT N'ProUserRoleSelectById patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\08_UserRoles\ProUserRoleSave.sql
GO
PRINT N'ProUserRoleSave patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\08_UserRoles\ProUserRoleCancel.sql
GO
PRINT N'ProUserRoleCancel patch successfully completed.';
GO

/* =============================================================================
   02_Users / 04_Auth / Patch.sql
   ============================================================================= */

:r .\02_Procedures\02_Users\04_Auth\RegisterUser.sql
GO
PRINT N'RegisterUser patch successfully completed.';
GO
:r .\02_Procedures\02_Users\04_Auth\GetUserByEmail.sql
GO
PRINT N'GetUserByEmail patch successfully completed.';
GO
:r .\02_Procedures\02_Users\04_Auth\GetUserById.sql
GO
PRINT N'GetUserById patch successfully completed.';
GO

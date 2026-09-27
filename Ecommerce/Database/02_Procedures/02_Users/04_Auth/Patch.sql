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
:r .\02_Procedures\02_Users\04_Auth\GetUserAddresses.sql
GO
PRINT N'GetUserAddresses patch successfully completed.';
GO
:r .\02_Procedures\02_Users\04_Auth\SaveUserAddress.sql
GO
PRINT N'SaveUserAddress patch successfully completed.';
GO
:r .\02_Procedures\02_Users\04_Auth\DeleteUserAddress.sql
GO
PRINT N'DeleteUserAddress patch successfully completed.';
GO

/* =============================================================================
   02_Users / 02_Cart / Patch.sql
   ============================================================================= */

:r .\02_Procedures\02_Users\02_Cart\GetBagCounts.sql
GO
PRINT N'GetBagCounts patch successfully completed.';
GO
:r .\02_Procedures\02_Users\02_Cart\GetCart.sql
GO
PRINT N'GetCart patch successfully completed.';
GO
:r .\02_Procedures\02_Users\02_Cart\AddCartItem.sql
GO
PRINT N'AddCartItem patch successfully completed.';
GO
:r .\02_Procedures\02_Users\02_Cart\UpdateCartItem.sql
GO
PRINT N'UpdateCartItem patch successfully completed.';
GO
:r .\02_Procedures\02_Users\02_Cart\RemoveCartItem.sql
GO
PRINT N'RemoveCartItem patch successfully completed.';
GO
:r .\02_Procedures\02_Users\02_Cart\ClearCart.sql
GO
PRINT N'ClearCart patch successfully completed.';
GO

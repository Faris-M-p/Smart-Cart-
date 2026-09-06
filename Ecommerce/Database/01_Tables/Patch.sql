/* =============================================================================
   01_Tables / Patch.sql
   -----------------------------------------------------------------------------
   Incremental table changes for an EXISTING SmartCart database (RunPatch.bat).
   Scripts here must be safe to re-run (IF NOT EXISTS / IF COL_LENGTH).
   Full CREATE TABLE scripts live in 01_Tables/Create.sql.
   ============================================================================= */

:r .\01_Tables\AdminUsers_AddProfileImageUrl.sql
GO
PRINT N'AdminUsers_AddProfileImageUrl patch successfully completed.';
GO
:r .\01_Tables\ProductMedia.sql
GO
PRINT N'ProductMedia patch successfully completed.';
GO
:r .\01_Tables\SkuMedia.sql
GO
PRINT N'SkuMedia patch successfully completed.';
GO
:r .\01_Tables\Cart_AddSessionKey.sql
GO
PRINT N'Cart_AddSessionKey patch successfully completed.';
GO
:r .\01_Tables\WishList_AddSessionKey.sql
GO
PRINT N'WishList_AddSessionKey patch successfully completed.';
GO
:r .\01_Tables\CartItems_AddProductVariantId.sql
GO
PRINT N'CartItems_AddProductVariantId patch successfully completed.';
GO
:r .\01_Tables\Users_AddFullName.sql
GO
PRINT N'Users_AddFullName patch successfully completed.';
GO

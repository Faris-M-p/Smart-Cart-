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
:r .\01_Tables\Orders_AddCheckoutFields.sql
GO
PRINT N'Orders_AddCheckoutFields patch successfully completed.';
GO
:r .\01_Tables\OrderItems.sql
GO
PRINT N'OrderItems patch successfully completed.';
GO
:r .\01_Tables\OrderItems_AlignKeys.sql
GO
PRINT N'OrderItems_AlignKeys patch successfully completed.';
GO
:r .\01_Tables\Categories.sql
GO
PRINT N'Categories patch successfully completed.';
GO
:r .\01_Tables\SubCategories.sql
GO
PRINT N'SubCategories patch successfully completed.';
GO
:r .\01_Tables\Brands.sql
GO
PRINT N'Brands patch successfully completed.';
GO
:r .\01_Tables\Products.sql
GO
PRINT N'Products patch successfully completed.';
GO
:r .\01_Tables\ProductVariants.sql
GO
PRINT N'ProductVariants patch successfully completed.';
GO

IF OBJECT_ID(N'[dbo].[GetStoreSettings]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[GetStoreSettings];
GO
IF OBJECT_ID(N'[dbo].[UpdateStoreSettings]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[UpdateStoreSettings];
GO
IF OBJECT_ID(N'[dbo].[StoreSettings]', N'U') IS NOT NULL
    DROP TABLE [dbo].[StoreSettings];
GO
PRINT N'StoreSettings removal completed.';
GO




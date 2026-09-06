/* =============================================================================
   SmartCart - Cart + Wishlist incremental apply
   Safe on an existing SmartCart database. Adds session/SKU columns and procedures.

   sqlcmd -S "(localdb)\MSSQLLocalDB" -d SmartCart -E -I -b -f 65001
          -i ApplyCartWishlist.sql
   ============================================================================= */

SET NOCOUNT ON;
GO

USE [SmartCart];
GO

PRINT N'=== Cart and wishlist patch starting ===';
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
:r .\02_Procedures\02_Users\03_Wishlist\GetWishlist.sql
GO
PRINT N'GetWishlist patch successfully completed.';
GO
:r .\02_Procedures\02_Users\03_Wishlist\GetWishlistStatus.sql
GO
PRINT N'GetWishlistStatus patch successfully completed.';
GO
:r .\02_Procedures\02_Users\03_Wishlist\ToggleWishlist.sql
GO
PRINT N'ToggleWishlist patch successfully completed.';
GO
:r .\02_Procedures\02_Users\03_Wishlist\RemoveWishlistItem.sql
GO
PRINT N'RemoveWishlistItem patch successfully completed.';
GO

PRINT N'=== Cart and wishlist patch completed successfully ===';
GO

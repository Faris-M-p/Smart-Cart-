/* =============================================================================
   02_Users / 03_Wishlist / Patch.sql
   ============================================================================= */

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

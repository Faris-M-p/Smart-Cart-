/* =============================================================================
   01_Tables / Create.sql
   -----------------------------------------------------------------------------
   Full database create only (RunDatabase.bat / Database.sql).
   Do not include this file from Database-Patch.sql.
   ============================================================================= */

:r .\01_Tables\Users.sql
GO
PRINT N'Users patch successfully completed.';
GO
:r .\01_Tables\Categories.sql
GO
PRINT N'Categories patch successfully completed.';
GO
:r .\01_Tables\Brands.sql
GO
PRINT N'Brands patch successfully completed.';
GO
:r .\01_Tables\Variants.sql
GO
PRINT N'Variants patch successfully completed.';
GO
:r .\01_Tables\ProductStatus.sql
GO
PRINT N'ProductStatus patch successfully completed.';
GO
:r .\01_Tables\AuditLogs.sql
GO
PRINT N'AuditLogs patch successfully completed.';
GO
:r .\01_Tables\Suppliers.sql
GO
PRINT N'Suppliers patch successfully completed.';
GO
:r .\01_Tables\SubCategories.sql
GO
PRINT N'SubCategories patch successfully completed.';
GO
:r .\01_Tables\VariantValues.sql
GO
PRINT N'VariantValues patch successfully completed.';
GO
:r .\01_Tables\Products.sql
GO
PRINT N'Products patch successfully completed.';
GO
:r .\01_Tables\ProductImages.sql
GO
PRINT N'ProductImages patch successfully completed.';
GO
:r .\01_Tables\ProductVariants.sql
GO
PRINT N'ProductVariants patch successfully completed.';
GO
:r .\01_Tables\Cart.sql
GO
PRINT N'Cart patch successfully completed.';
GO
:r .\01_Tables\WishList.sql
GO
PRINT N'WishList patch successfully completed.';
GO
:r .\01_Tables\Orders.sql
GO
PRINT N'Orders patch successfully completed.';
GO
:r .\01_Tables\Ratings.sql
GO
PRINT N'Ratings patch successfully completed.';
GO
:r .\01_Tables\ProductVariantImages.sql
GO
PRINT N'ProductVariantImages patch successfully completed.';
GO
:r .\01_Tables\ProductVariantAttributes.sql
GO
PRINT N'ProductVariantAttributes patch successfully completed.';
GO
:r .\01_Tables\CartItems.sql
GO
PRINT N'CartItems patch successfully completed.';
GO
:r .\01_Tables\WishListItems.sql
GO
PRINT N'WishListItems patch successfully completed.';
GO
:r .\01_Tables\Payments.sql
GO
PRINT N'Payments patch successfully completed.';
GO
:r .\01_Tables\Shipping.sql
GO
PRINT N'Shipping patch successfully completed.';
GO
:r .\01_Tables\Purchases.sql
GO
PRINT N'Purchases patch successfully completed.';
GO
:r .\01_Tables\PurchaseDetails.sql
GO
PRINT N'PurchaseDetails patch successfully completed.';
GO
:r .\01_Tables\Stock.sql
GO
PRINT N'Stock patch successfully completed.';
GO
:r .\01_Tables\UserRoles.sql
GO
PRINT N'UserRoles patch successfully completed.';
GO
:r .\01_Tables\Modules.sql
GO
PRINT N'Modules patch successfully completed.';
GO
:r .\01_Tables\Permissions.sql
GO
PRINT N'Permissions patch successfully completed.';
GO
:r .\01_Tables\AdminUsers.sql
GO
PRINT N'AdminUsers patch successfully completed.';
GO
:r .\01_Tables\UserRolePermissions.sql
GO
PRINT N'UserRolePermissions patch successfully completed.';
GO

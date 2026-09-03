/* =============================================================================
   01_Tables / Patch.sql
   -----------------------------------------------------------------------------
   Controls which table scripts execute.
   sqlcmd resolves :r paths from the Database folder (RunDatabase.bat / RunPatch.bat).

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment the files that should not run; leave only the
                    required table scripts uncommented.
   ============================================================================= */

:r .\01_Tables\Users.sql
:r .\01_Tables\Categories.sql
:r .\01_Tables\Brands.sql
:r .\01_Tables\Variants.sql
:r .\01_Tables\ProductStatus.sql
:r .\01_Tables\AuditLogs.sql
:r .\01_Tables\Suppliers.sql
:r .\01_Tables\SubCategories.sql
:r .\01_Tables\VariantValues.sql
:r .\01_Tables\Products.sql
:r .\01_Tables\ProductImages.sql
:r .\01_Tables\ProductVariants.sql
:r .\01_Tables\Cart.sql
:r .\01_Tables\WishList.sql
:r .\01_Tables\Orders.sql
:r .\01_Tables\Ratings.sql
:r .\01_Tables\ProductVariantImages.sql
:r .\01_Tables\ProductVariantAttributes.sql
:r .\01_Tables\CartItems.sql
:r .\01_Tables\WishListItems.sql
:r .\01_Tables\Payments.sql
:r .\01_Tables\Shipping.sql
:r .\01_Tables\Purchases.sql
:r .\01_Tables\PurchaseDetails.sql
:r .\01_Tables\Stock.sql

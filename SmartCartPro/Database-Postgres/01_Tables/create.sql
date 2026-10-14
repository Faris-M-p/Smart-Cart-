-- SmartCartPro table create order (dependency-safe)
-- Paths are relative to this file (\ir = include relative).
\set ON_ERROR_STOP on

\echo Creating 001_Company.sql
\ir ./001_Company.sql

\echo Creating 002_Store.sql
\ir ./002_Store.sql

\echo Creating 003_Users.sql
\ir ./003_Users.sql

\echo Creating 004_Roles.sql
\ir ./004_Roles.sql

\echo Creating 005_Modules.sql
\ir ./005_Modules.sql

\echo Creating 006_Permissions.sql
\ir ./006_Permissions.sql

\echo Creating 007_RolePermissions.sql
\ir ./007_RolePermissions.sql

\echo Creating 008_UserRoles.sql
\ir ./008_UserRoles.sql

\echo Creating 009_UserStoreAccess.sql
\ir ./009_UserStoreAccess.sql

\echo Creating 010_Units.sql
\ir ./010_Units.sql

\echo Creating 011_Taxes.sql
\ir ./011_Taxes.sql

\echo Creating 012_Categories.sql
\ir ./012_Categories.sql

\echo Creating 013_SubCategories.sql
\ir ./013_SubCategories.sql

\echo Creating 014_Brands.sql
\ir ./014_Brands.sql

\echo Creating 015_Products.sql
\ir ./015_Products.sql

\echo Creating 016_ProductVariants.sql
\ir ./016_ProductVariants.sql

\echo Creating 017_ProductImages.sql
\ir ./017_ProductImages.sql

\echo Creating 018_Suppliers.sql
\ir ./018_Suppliers.sql

\echo Creating 019_Customers.sql
\ir ./019_Customers.sql

\echo Creating 020_CustomerAddresses.sql
\ir ./020_CustomerAddresses.sql

\echo Creating 021_Inventory.sql
\ir ./021_Inventory.sql

\echo Creating 022_StockMovements.sql
\ir ./022_StockMovements.sql

\echo Creating 023_Purchases.sql
\ir ./023_Purchases.sql

\echo Creating 024_PurchaseDetails.sql
\ir ./024_PurchaseDetails.sql

\echo Creating 027_CashSessions.sql
\ir ./027_CashSessions.sql

\echo Creating 025_Sales.sql
\ir ./025_Sales.sql

\echo Creating 026_SaleDetails.sql
\ir ./026_SaleDetails.sql

\echo Creating 028_Payments.sql
\ir ./028_Payments.sql

\echo Creating 029_SalesReturns.sql
\ir ./029_SalesReturns.sql

\echo Creating 030_SalesReturnDetails.sql
\ir ./030_SalesReturnDetails.sql

\echo Creating 031_Expenses.sql
\ir ./031_Expenses.sql

\echo Creating 032_Settings.sql
\ir ./032_Settings.sql

\echo Creating 033_AuditLogs.sql
\ir ./033_AuditLogs.sql

\echo Creating 034_OnlineOrders.sql
\ir ./034_OnlineOrders.sql

\echo Creating 035_OnlineOrderItems.sql
\ir ./035_OnlineOrderItems.sql

\echo Creating 036_Carts.sql
\ir ./036_Carts.sql

\echo Creating 037_CartItems.sql
\ir ./037_CartItems.sql

\echo Creating 038_Wishlists.sql
\ir ./038_Wishlists.sql

\echo Creating 039_WishlistItems.sql
\ir ./039_WishlistItems.sql

\echo Creating 040_ProductReviews.sql
\ir ./040_ProductReviews.sql

\echo Creating 041_InvoiceSequences.sql
\ir ./041_InvoiceSequences.sql

\echo Creating 042_PatchHistory.sql
\ir ./042_PatchHistory.sql

-- Wire sales.fk_cashsession after cash_sessions exists
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_sales_cashsession'
    ) THEN
        ALTER TABLE sales
            ADD CONSTRAINT fk_sales_cashsession
            FOREIGN KEY (fk_cashsession) REFERENCES cash_sessions (id_cashsession);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_sales_fk_cashsession ON sales (fk_cashsession);

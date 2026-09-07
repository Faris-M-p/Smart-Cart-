/* =============================================================================
   02_Users / 05_Orders / Patch.sql
   ============================================================================= */

:r .\02_Procedures\02_Users\05_Orders\GetCheckoutPreview.sql
GO
PRINT N'GetCheckoutPreview patch successfully completed.';
GO
:r .\02_Procedures\02_Users\05_Orders\PlaceOrder.sql
GO
PRINT N'PlaceOrder patch successfully completed.';
GO
:r .\02_Procedures\02_Users\05_Orders\GetOrder.sql
GO
PRINT N'GetOrder patch successfully completed.';
GO
:r .\02_Procedures\02_Users\05_Orders\GetOrders.sql
GO
PRINT N'GetOrders patch successfully completed.';
GO
:r .\02_Procedures\02_Users\05_Orders\CancelOrder.sql
GO
PRINT N'CancelOrder patch successfully completed.';
GO

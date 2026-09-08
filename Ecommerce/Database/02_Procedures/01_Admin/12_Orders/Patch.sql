/* =============================================================================
   01_Admin / 12_Orders / Patch.sql
   -----------------------------------------------------------------------------
   Controls which admin order procedure scripts execute.
   sqlcmd resolves :r paths from the Database folder.

   INITIAL CREATE : keep every :r line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

:r .\02_Procedures\01_Admin\12_Orders\GetAdminOrders.sql
GO
PRINT N'GetAdminOrders patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\12_Orders\GetAdminOrder.sql
GO
PRINT N'GetAdminOrder patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\12_Orders\AdminConfirmOrder.sql
GO
PRINT N'AdminConfirmOrder patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\12_Orders\AdminUpdateOrderStatus.sql
GO
PRINT N'AdminUpdateOrderStatus patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\12_Orders\AdminDeliverOrder.sql
GO
PRINT N'AdminDeliverOrder patch successfully completed.';
GO
:r .\02_Procedures\01_Admin\12_Orders\AdminCancelOrder.sql
GO
PRINT N'AdminCancelOrder patch successfully completed.';
GO

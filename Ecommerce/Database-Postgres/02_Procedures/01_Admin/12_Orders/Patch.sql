/* =============================================================================
   01_Admin / 12_Orders / Patch.sql
   -----------------------------------------------------------------------------
   psql resolves \i paths from the Database-Postgres folder.

   INITIAL CREATE : keep every \i line uncommented.
   PATCH / UPDATE : comment files that should not run.
   ============================================================================= */

\echo '--- get_admin_orders ---'
\i ./02_Procedures/01_Admin/12_Orders/get_admin_orders.sql
\echo 'get_admin_orders patch successfully completed.'

\echo '--- get_admin_order ---'
\i ./02_Procedures/01_Admin/12_Orders/get_admin_order.sql
\echo 'get_admin_order patch successfully completed.'

\echo '--- admin_confirm_order ---'
\i ./02_Procedures/01_Admin/12_Orders/admin_confirm_order.sql
\echo 'admin_confirm_order patch successfully completed.'

\echo '--- admin_update_order_status ---'
\i ./02_Procedures/01_Admin/12_Orders/admin_update_order_status.sql
\echo 'admin_update_order_status patch successfully completed.'

\echo '--- admin_deliver_order ---'
\i ./02_Procedures/01_Admin/12_Orders/admin_deliver_order.sql
\echo 'admin_deliver_order patch successfully completed.'

\echo '--- admin_cancel_order ---'
\i ./02_Procedures/01_Admin/12_Orders/admin_cancel_order.sql
\echo 'admin_cancel_order patch successfully completed.'

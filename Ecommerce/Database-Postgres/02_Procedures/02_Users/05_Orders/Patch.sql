/* =============================================================================
   02_Users / 05_Orders / Patch.sql
   ============================================================================= */

\i ./02_Procedures/02_Users/05_Orders/get_checkout_preview.sql
\echo 'get_checkout_preview patch successfully completed.'

\i ./02_Procedures/02_Users/05_Orders/place_order.sql
\echo 'place_order patch successfully completed.'

\i ./02_Procedures/02_Users/05_Orders/get_order.sql
\echo 'get_order patch successfully completed.'

\i ./02_Procedures/02_Users/05_Orders/get_orders.sql
\echo 'get_orders patch successfully completed.'

\i ./02_Procedures/02_Users/05_Orders/cancel_order.sql
\echo 'cancel_order patch successfully completed.'

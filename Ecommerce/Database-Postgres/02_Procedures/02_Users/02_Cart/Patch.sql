/* =============================================================================
   02_Users / 02_Cart / Patch.sql
   ============================================================================= */

\i ./02_Procedures/02_Users/02_Cart/get_bag_counts.sql
\echo 'get_bag_counts patch successfully completed.'

\i ./02_Procedures/02_Users/02_Cart/get_cart.sql
\echo 'get_cart patch successfully completed.'

\i ./02_Procedures/02_Users/02_Cart/add_cart_item.sql
\echo 'add_cart_item patch successfully completed.'

\i ./02_Procedures/02_Users/02_Cart/update_cart_item.sql
\echo 'update_cart_item patch successfully completed.'

\i ./02_Procedures/02_Users/02_Cart/remove_cart_item.sql
\echo 'remove_cart_item patch successfully completed.'

\i ./02_Procedures/02_Users/02_Cart/clear_cart.sql
\echo 'clear_cart patch successfully completed.'

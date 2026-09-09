/* =============================================================================
   02_Users / 03_Wishlist / Patch.sql
   ============================================================================= */

\i ./02_Procedures/02_Users/03_Wishlist/get_wishlist.sql
\echo 'get_wishlist patch successfully completed.'

\i ./02_Procedures/02_Users/03_Wishlist/get_wishlist_status.sql
\echo 'get_wishlist_status patch successfully completed.'

\i ./02_Procedures/02_Users/03_Wishlist/toggle_wishlist.sql
\echo 'toggle_wishlist patch successfully completed.'

\i ./02_Procedures/02_Users/03_Wishlist/remove_wishlist_item.sql
\echo 'remove_wishlist_item patch successfully completed.'

/* =============================================================================
   01_Tables / create.sql — full create (Database.sql only)
   ============================================================================= */

\echo '--- users ---'
\i ./01_Tables/users.sql
\echo 'users completed.'

\echo '--- category ---'
\i ./01_Tables/category.sql
\echo 'category completed.'

\echo '--- brand ---'
\i ./01_Tables/brand.sql
\echo 'brand completed.'

\echo '--- variants ---'
\i ./01_Tables/variants.sql
\echo 'variants completed.'

\echo '--- product_status ---'
\i ./01_Tables/product_status.sql
\echo 'product_status completed.'

\echo '--- audit_logs ---'
\i ./01_Tables/audit_logs.sql
\echo 'audit_logs completed.'

\echo '--- supplier ---'
\i ./01_Tables/supplier.sql
\echo 'supplier completed.'

\echo '--- subcategory ---'
\i ./01_Tables/subcategory.sql
\echo 'subcategory completed.'

\echo '--- variant_values ---'
\i ./01_Tables/variant_values.sql
\echo 'variant_values completed.'

\echo '--- products ---'
\i ./01_Tables/products.sql
\echo 'products completed.'

\echo '--- product_images ---'
\i ./01_Tables/product_images.sql
\echo 'product_images completed.'

\echo '--- product_variants ---'
\i ./01_Tables/product_variants.sql
\echo 'product_variants completed.'

\echo '--- cart ---'
\i ./01_Tables/cart.sql
\echo 'cart completed.'

\echo '--- wishlist ---'
\i ./01_Tables/wishlist.sql
\echo 'wishlist completed.'

\echo '--- orders ---'
\i ./01_Tables/orders.sql
\echo 'orders completed.'

\echo '--- order_items ---'
\i ./01_Tables/order_items.sql
\echo 'order_items completed.'

\echo '--- ratings ---'
\i ./01_Tables/ratings.sql
\echo 'ratings completed.'

\echo '--- product_variant_images ---'
\i ./01_Tables/product_variant_images.sql
\echo 'product_variant_images completed.'

\echo '--- product_variant_attributes ---'
\i ./01_Tables/product_variant_attributes.sql
\echo 'product_variant_attributes completed.'

\echo '--- cart_items ---'
\i ./01_Tables/cart_items.sql
\echo 'cart_items completed.'

\echo '--- wishlist_items ---'
\i ./01_Tables/wishlist_items.sql
\echo 'wishlist_items completed.'

\echo '--- payments ---'
\i ./01_Tables/payments.sql
\echo 'payments completed.'

\echo '--- shipping ---'
\i ./01_Tables/shipping.sql
\echo 'shipping completed.'

\echo '--- purchase ---'
\i ./01_Tables/purchase.sql
\echo 'purchase completed.'

\echo '--- purchase_detail ---'
\i ./01_Tables/purchase_detail.sql
\echo 'purchase_detail completed.'

\echo '--- stock ---'
\i ./01_Tables/stock.sql
\echo 'stock completed.'

\echo '--- user_roles ---'
\i ./01_Tables/user_roles.sql
\echo 'user_roles completed.'

\echo '--- modules ---'
\i ./01_Tables/modules.sql
\echo 'modules completed.'

\echo '--- permissions ---'
\i ./01_Tables/permissions.sql
\echo 'permissions completed.'

\echo '--- admin_users ---'
\i ./01_Tables/admin_users.sql
\echo 'admin_users completed.'

\echo '--- user_role_permissions ---'
\i ./01_Tables/user_role_permissions.sql
\echo 'user_role_permissions completed.'

\echo '--- product_media ---'
\i ./01_Tables/product_media.sql
\echo 'product_media completed.'

\echo '--- sku_media ---'
\i ./01_Tables/sku_media.sql
\echo 'sku_media completed.'

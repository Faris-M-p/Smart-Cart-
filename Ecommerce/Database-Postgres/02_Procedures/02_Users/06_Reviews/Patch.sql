/* =============================================================================
   02_Users / 06_Reviews / Patch.sql
   Order-item based ratings & reviews (existing ratings table).
   ============================================================================= */

-- The first review release used product-based procedures. Drop every overload of
-- the old/changed names so no stale (non order-item) version stays callable.
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT p.oid::regprocedure AS sig
        FROM pg_proc AS p
        INNER JOIN pg_namespace AS n ON n.oid = p.pronamespace
        WHERE n.nspname = current_schema()
          AND p.prokind = 'p'
          AND p.proname IN (
              'get_product_reviews',
              'submit_product_review',
              'update_product_review',
              'delete_product_review'
          )
    LOOP
        EXECUTE 'DROP PROCEDURE IF EXISTS ' || r.sig::TEXT;
    END LOOP;
END $$;

\i ./02_Procedures/02_Users/06_Reviews/get_product_reviews.sql
\echo 'get_product_reviews patch successfully completed.'

\i ./02_Procedures/02_Users/06_Reviews/get_order_item_reviews.sql
\echo 'get_order_item_reviews patch successfully completed.'

\i ./02_Procedures/02_Users/06_Reviews/submit_order_item_review.sql
\echo 'submit_order_item_review patch successfully completed.'

\i ./02_Procedures/02_Users/06_Reviews/update_order_item_review.sql
\echo 'update_order_item_review patch successfully completed.'

\i ./02_Procedures/02_Users/06_Reviews/delete_order_item_review.sql
\echo 'delete_order_item_review patch successfully completed.'

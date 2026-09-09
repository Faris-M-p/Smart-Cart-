/* =============================================================================
   Procedure : get_bag_counts
   Source    : GetBagCounts (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_bag_counts(
    p_user_id      INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        COALESCE((
            SELECT COUNT(1)
            FROM cart_items AS ci
            INNER JOIN cart AS c ON c.cart_id = ci.cart_id
            WHERE c.user_id = p_user_id
        ), 0) AS "CartCount",
        COALESCE((
            SELECT COUNT(1)
            FROM wishlist_items AS wi
            INNER JOIN wishlist AS w ON w.wishlist_id = wi.wishlist_id
            WHERE w.user_id = p_user_id
              AND COALESCE(w.cancelled, FALSE) = FALSE
              AND COALESCE(wi.cancelled, FALSE) = FALSE
        ), 0) AS "WishlistCount";
END;
$$;

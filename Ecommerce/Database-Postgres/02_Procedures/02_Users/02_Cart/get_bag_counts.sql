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
            FROM cartitems AS ci
            INNER JOIN cart AS c ON c.id_cart = ci.fk_cart
            WHERE c.fk_user = p_user_id
        ), 0) AS cartcount,
        COALESCE((
            SELECT COUNT(1)
            FROM wishlistitems AS wi
            INNER JOIN wishlist AS w ON w.id_wishlist = wi.fk_wishlist
            WHERE w.fk_user = p_user_id
              AND COALESCE(w.cancelled, FALSE) = FALSE
              AND COALESCE(wi.cancelled, FALSE) = FALSE
        ), 0) AS wishlistcount;
END;
$$;

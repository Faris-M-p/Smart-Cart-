/* =============================================================================
   Procedure : get_wishlist_status
   Source    : GetWishlistStatus (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE get_wishlist_status(
    p_user_id      INT,
    p_product_id   INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
BEGIN
    OPEN p_result FOR
    SELECT
        CASE WHEN EXISTS (
            SELECT 1
            FROM wishlist_items AS wi
            INNER JOIN wishlist AS w ON w.wishlist_id = wi.wishlist_id
            WHERE w.user_id = p_user_id
              AND wi.product_id = p_product_id
              AND COALESCE(w.cancelled, FALSE) = FALSE
              AND COALESCE(wi.cancelled, FALSE) = FALSE
        ) THEN TRUE ELSE FALSE END AS "InWishlist";
END;
$$;

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
            FROM wishlistitems AS wi
            INNER JOIN wishlist AS w ON w.id_wishlist = wi.fk_wishlist
            WHERE w.fk_user = p_user_id
              AND wi.fk_product = p_product_id
              AND COALESCE(w.cancelled, FALSE) = FALSE
              AND COALESCE(wi.cancelled, FALSE) = FALSE
        ) THEN TRUE ELSE FALSE END AS inwishlist;
END;
$$;

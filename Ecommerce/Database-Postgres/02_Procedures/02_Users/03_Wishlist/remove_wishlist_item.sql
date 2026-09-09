/* =============================================================================
   Procedure : remove_wishlist_item
   Source    : RemoveWishlistItem (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE remove_wishlist_item(
    p_user_id          INT,
    p_wishlist_item_id INT,
    INOUT p_result     refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_rowcount INT;
BEGIN
    UPDATE wishlist_items AS wi
    SET cancelled = TRUE,
        cancelled_on = NOW(),
        cancelled_reason = 'Removed from wishlist'
    FROM wishlist AS w
    WHERE w.wishlist_id = wi.wishlist_id
      AND wi.wishlist_item_id = p_wishlist_item_id
      AND w.user_id = p_user_id
      AND COALESCE(wi.cancelled, FALSE) = FALSE
      AND COALESCE(w.cancelled, FALSE) = FALSE;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Wishlist item was not found.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    OPEN p_result FOR
    SELECT 0 AS "ResponseCode", 1 AS "StatusCode",
           'Removed from wishlist.'::TEXT AS "ResponseMsg";
END;
$$;

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
    UPDATE wishlistitems AS wi
    SET cancelled = TRUE,
        cancelledon = NOW(),
        cancelledreason = 'Removed from wishlist'
    FROM wishlist AS w
    WHERE w.id_wishlist = wi.fk_wishlist
      AND wi.id_wishlistitem = p_wishlist_item_id
      AND w.fk_user = p_user_id
      AND COALESCE(wi.cancelled, FALSE) = FALSE
      AND COALESCE(w.cancelled, FALSE) = FALSE;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        OPEN p_result FOR
        SELECT -1 AS responsecode, 0 AS statuscode,
               'Wishlist item was not found.'::TEXT AS responsemsg;
        RETURN;
    END IF;

    OPEN p_result FOR
    SELECT 0 AS responsecode, 1 AS statuscode,
           'Removed from wishlist.'::TEXT AS responsemsg;
END;
$$;

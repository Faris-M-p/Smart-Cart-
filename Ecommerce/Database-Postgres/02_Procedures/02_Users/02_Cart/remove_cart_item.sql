/* =============================================================================
   Procedure : remove_cart_item
   Source    : RemoveCartItem (SQL Server)
   ============================================================================= */

CREATE OR REPLACE PROCEDURE remove_cart_item(
    p_user_id      INT,
    p_cart_item_id INT,
    INOUT p_result refcursor DEFAULT 'p_result'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_rowcount INT;
BEGIN
    DELETE FROM cart_items AS ci
    USING cart AS c
    WHERE c.cart_id = ci.cart_id
      AND ci.cart_item_id = p_cart_item_id
      AND c.user_id = p_user_id;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        OPEN p_result FOR
        SELECT -1 AS "ResponseCode", 0 AS "StatusCode",
               'Cart item was not found.'::TEXT AS "ResponseMsg";
        RETURN;
    END IF;

    OPEN p_result FOR
    SELECT 0 AS "ResponseCode", 1 AS "StatusCode",
           'Item removed from cart.'::TEXT AS "ResponseMsg";
END;
$$;
